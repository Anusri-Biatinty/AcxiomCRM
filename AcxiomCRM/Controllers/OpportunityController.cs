using AcxiomCRM.Models.Identity;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels.Opportunity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class OpportunityController : Controller
    {
        private readonly IOpportunityService           _svc;
        private readonly ICustomerService              _custSvc;
        private readonly IAuditService                 _audit;
        private readonly UserManager<ApplicationUser> _users;

        public OpportunityController(IOpportunityService svc, ICustomerService custSvc,
            IAuditService audit, UserManager<ApplicationUser> users)
        {
            _svc     = svc;
            _custSvc = custSvc;
            _audit   = audit;
            _users   = users;
        }

        // GET /Opportunity
        public async Task<IActionResult> Index(string? search, string? stage)
        {
            var user  = await _users.GetUserAsync(User);
            var roles = await _users.GetRolesAsync(user!);
            bool isPriv = roles.Contains("Admin") || roles.Contains("Manager");

            var list = await _svc.GetAllAsync(user!.Id, isPriv);

            if (!string.IsNullOrWhiteSpace(search))
                list = list.Where(o =>
                    o.OpportunityName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (o.Customer?.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();

            if (!string.IsNullOrWhiteSpace(stage))
                list = list.Where(o => o.Stage == stage).ToList();

            ViewBag.Search = search;
            ViewBag.Stage  = stage;
            return View(list);
        }

        // GET /Opportunity/Create
        [HttpGet]
        public async Task<IActionResult> Create()
            => View(new OpportunityViewModel
            {
                Customers  = await GetCustomerList(),
                SalesUsers = await GetSalesUserList()
            });

        // POST /Opportunity/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OpportunityViewModel model)
        {
            // ── Business validation (server-enforced regardless of client) ────
            if (model.Amount <= 0)
                ModelState.AddModelError("Amount",
                    "Opportunity Amount must be greater than 0.");

            if (model.Probability < 0 || model.Probability > 100)
                ModelState.AddModelError("Probability",
                    "Probability must be between 0 and 100.");

            if (model.ExpectedCloseDate.Date < DateTime.Today &&
                model.Stage != "Won" && model.Stage != "Lost")
                ModelState.AddModelError("ExpectedCloseDate",
                    "Expected Close Date cannot be in the past.");

            if (!ModelState.IsValid)
            {
                model.Customers  = await GetCustomerList();
                model.SalesUsers = await GetSalesUserList();
                return View(model);
            }

            var user = await _users.GetUserAsync(User);
            if (string.IsNullOrEmpty(model.AssignedTo)) model.AssignedTo = user!.Id;

            var opp = await _svc.CreateAsync(model, user!.Id);
            var ip  = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _audit.LogAsync(user.Id, user.Email ?? "", "Create", "Opportunity",
                opp.OpportunityId.ToString(), ipAddress: ip);

            TempData["Success"] = $"Opportunity '{opp.OpportunityName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Opportunity/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var o = await _svc.GetByIdAsync(id);
            if (o == null) return NotFound();

            return View(new OpportunityViewModel
            {
                OpportunityId    = o.OpportunityId,
                OpportunityName  = o.OpportunityName,
                CustomerId       = o.CustomerId,
                LeadId           = o.LeadId,
                Stage            = o.Stage,
                Amount           = o.Amount,
                Probability      = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Notes            = o.Notes,
                Status           = o.Status,
                AssignedTo       = o.AssignedTo,
                Customers        = await GetCustomerList(),
                SalesUsers       = await GetSalesUserList()
            });
        }

        // POST /Opportunity/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OpportunityViewModel model)
        {
            if (model.Amount <= 0)
                ModelState.AddModelError("Amount",
                    "Opportunity Amount must be greater than 0.");

            if (model.Probability < 0 || model.Probability > 100)
                ModelState.AddModelError("Probability",
                    "Probability must be between 0 and 100.");

            if (model.ExpectedCloseDate.Date < DateTime.Today &&
                model.Stage != "Won" && model.Stage != "Lost")
                ModelState.AddModelError("ExpectedCloseDate",
                    "Expected Close Date cannot be in the past.");

            if (!ModelState.IsValid)
            {
                model.Customers  = await GetCustomerList();
                model.SalesUsers = await GetSalesUserList();
                return View(model);
            }

            var old  = await _svc.GetByIdAsync(model.OpportunityId);
            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _svc.UpdateAsync(model);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Update", "Opportunity",
                model.OpportunityId.ToString(),
                oldValue: $"Stage={old?.Stage},Amount={old?.Amount}",
                newValue: $"Stage={model.Stage},Amount={model.Amount}",
                ipAddress: ip);

            TempData["Success"] = "Opportunity updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Opportunity/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var o = await _svc.GetByIdAsync(id);
            if (o == null) return NotFound();
            return View(o);
        }

        // POST /Opportunity/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _svc.DeleteAsync(id);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Delete", "Opportunity",
                id.ToString(), ipAddress: ip);

            TempData["Success"] = "Opportunity deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetCustomerList()
        {
            var customers = await _custSvc.GetAllAsync(isPrivileged: true);
            return customers
                .Where(c => c.Status == "Active")
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text  = c.CustomerName
                }).ToList();
        }

        private async Task<List<SelectListItem>> GetSalesUserList()
        {
            var salesUsers = await _users.GetUsersInRoleAsync("SalesExecutive");
            return salesUsers
                .Where(u => u.IsActive)
                .Select(u => new SelectListItem { Value = u.Id, Text = u.FullName })
                .ToList();
        }
    }
}
