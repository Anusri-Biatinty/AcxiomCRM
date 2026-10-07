using AcxiomCRM.Models.Identity;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService              _svc;
        private readonly IAuditService                 _audit;
        private readonly UserManager<ApplicationUser> _users;

        public CustomerController(ICustomerService svc, IAuditService audit,
            UserManager<ApplicationUser> users)
        {
            _svc   = svc;
            _audit = audit;
            _users = users;
        }

        // GET /Customer
        public async Task<IActionResult> Index(string? name, string? email,
            string? phone, string? company, string? status)
        {
            var user  = await _users.GetUserAsync(User);
            var roles = await _users.GetRolesAsync(user!);
            bool isPriv = roles.Contains("Admin") || roles.Contains("Manager");

            var list = await _svc.GetAllAsync(user!.Id, isPriv);

            if (!string.IsNullOrWhiteSpace(name))
                list = list.Where(c => c.CustomerName.Contains(name,
                    StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(email))
                list = list.Where(c => c.Email.Contains(email,
                    StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(phone))
                list = list.Where(c => c.Phone.Contains(phone)).ToList();
            if (!string.IsNullOrWhiteSpace(company))
                list = list.Where(c => c.CompanyName.Contains(company,
                    StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(status))
                list = list.Where(c => c.Status == status).ToList();

            ViewBag.Name    = name;
            ViewBag.Email   = email;
            ViewBag.Phone   = phone;
            ViewBag.Company = company;
            ViewBag.Status  = status;
            return View(list);
        }

        // GET /Customer/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CustomerViewModel
            {
                SalesUsers = await GetSalesUserList()
            };
            return View(vm);
        }

        // POST /Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            // Server-side duplicate checks (not catchable by data annotations)
            if (await _svc.EmailExistsAsync(model.Email))
                ModelState.AddModelError("Email",
                    "A customer with this email already exists.");

            if (await _svc.PhoneExistsAsync(model.Phone))
                ModelState.AddModelError("Phone",
                    "A customer with this phone number already exists.");

            if (!ModelState.IsValid)
            {
                model.SalesUsers = await GetSalesUserList();
                return View(model);
            }

            var user     = await _users.GetUserAsync(User);
            var customer = await _svc.CreateAsync(model, user!.Id);
            var ip       = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _audit.LogAsync(user.Id, user.Email ?? "", "Create", "Customer",
                customer.CustomerId.ToString(), ipAddress: ip);

            TempData["Success"] = $"Customer {customer.CustomerName} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Customer/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _svc.GetByIdAsync(id);
            if (c == null) return NotFound();

            return View(new CustomerViewModel
            {
                CustomerId   = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email        = c.Email,
                Phone        = c.Phone,
                CompanyName  = c.CompanyName,
                Address      = c.Address,
                City         = c.City,
                State        = c.State,
                Status       = c.Status,
                AssignedTo   = c.AssignedTo,
                SalesUsers   = await GetSalesUserList()
            });
        }

        // POST /Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerViewModel model)
        {
            if (await _svc.EmailExistsAsync(model.Email, model.CustomerId))
                ModelState.AddModelError("Email",
                    "A customer with this email already exists.");

            if (await _svc.PhoneExistsAsync(model.Phone, model.CustomerId))
                ModelState.AddModelError("Phone",
                    "A customer with this phone number already exists.");

            if (!ModelState.IsValid)
            {
                model.SalesUsers = await GetSalesUserList();
                return View(model);
            }

            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _svc.UpdateAsync(model);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Update", "Customer",
                model.CustomerId.ToString(), ipAddress: ip);

            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Customer/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var c = await _svc.GetByIdAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        // POST /Customer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _svc.DeactivateAsync(id);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Deactivate", "Customer",
                id.ToString(), ipAddress: ip);

            TempData["Success"] = "Customer deactivated.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetSalesUserList()
        {
            var salesUsers = await _users.GetUsersInRoleAsync("SalesExecutive");
            return salesUsers
                .Where(u => u.IsActive)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text  = u.FullName
                }).ToList();
        }
    }
}
