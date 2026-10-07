using AcxiomCRM.Models.Identity;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels.Lead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class LeadController : Controller
    {
        private static readonly string[] ValidStatuses =
            { "New", "Contacted", "Qualified", "Unqualified", "Converted", "Lost" };

        private readonly ILeadService                  _svc;
        private readonly IAuditService                 _audit;
        private readonly UserManager<ApplicationUser> _users;

        public LeadController(ILeadService svc, IAuditService audit,
            UserManager<ApplicationUser> users)
        {
            _svc   = svc;
            _audit = audit;
            _users = users;
        }

        // GET /Lead
        public async Task<IActionResult> Index(string? search, string? status, string? priority)
        {
            var user  = await _users.GetUserAsync(User);
            var roles = await _users.GetRolesAsync(user!);
            bool isPriv = roles.Contains("Admin") || roles.Contains("Manager");

            var list = await _svc.GetAllAsync(user!.Id, isPriv);

            if (!string.IsNullOrWhiteSpace(search))
                list = list.Where(l =>
                    l.LeadName.Contains(search,  StringComparison.OrdinalIgnoreCase) ||
                    l.Email.Contains(search,     StringComparison.OrdinalIgnoreCase) ||
                    l.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(status))
                list = list.Where(l => l.Status == status).ToList();

            if (!string.IsNullOrWhiteSpace(priority))
                list = list.Where(l => l.Priority == priority).ToList();

            ViewBag.Search   = search;
            ViewBag.Status   = status;
            ViewBag.Priority = priority;
            return View(list);
        }

        // GET /Lead/Create
        [HttpGet]
        public async Task<IActionResult> Create()
            => View(new LeadViewModel { SalesUsers = await GetSalesUserList() });

        // POST /Lead/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeadViewModel model)
        {
            // Server-side: status must be from valid set
            if (!ValidStatuses.Contains(model.Status))
                ModelState.AddModelError("Status", "Invalid lead status selected.");

            if (!ModelState.IsValid)
            {
                model.SalesUsers = await GetSalesUserList();
                return View(model);
            }

            var user = await _users.GetUserAsync(User);
            var lead = await _svc.CreateAsync(model, user!.Id);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _audit.LogAsync(user.Id, user.Email ?? "", "Create", "Lead",
                lead.LeadId.ToString(), ipAddress: ip);

            TempData["Success"] = $"Lead {lead.LeadName} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Lead/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var l = await _svc.GetByIdAsync(id);
            if (l == null) return NotFound();

            return View(new LeadViewModel
            {
                LeadId        = l.LeadId,
                LeadCode      = l.LeadCode,
                LeadName      = l.LeadName,
                Email         = l.Email,
                Phone         = l.Phone,
                CompanyName   = l.CompanyName,
                Source        = l.Source,
                Status        = l.Status,
                Priority      = l.Priority,
                ExpectedValue = l.ExpectedValue,
                AssignedTo    = l.AssignedTo,
                SalesUsers    = await GetSalesUserList()
            });
        }

        // POST /Lead/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LeadViewModel model)
        {
            if (!ValidStatuses.Contains(model.Status))
                ModelState.AddModelError("Status", "Invalid lead status selected.");

            if (!ModelState.IsValid)
            {
                model.SalesUsers = await GetSalesUserList();
                return View(model);
            }

            var oldLead = await _svc.GetByIdAsync(model.LeadId);
            string oldStatus = oldLead?.Status ?? "";

            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _svc.UpdateAsync(model);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Update", "Lead",
                model.LeadId.ToString(),
                oldValue: $"Status={oldStatus}", newValue: $"Status={model.Status}",
                ipAddress: ip);

            TempData["Success"] = "Lead updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Lead/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var l = await _svc.GetByIdAsync(id);
            if (l == null) return NotFound();
            return View(l);
        }

        // POST /Lead/Convert/5  — lead-to-customer conversion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Convert(int id)
        {
            var lead = await _svc.GetByIdAsync(id);
            if (lead == null) return NotFound();

            if (lead.Status != "Qualified")
            {
                TempData["Error"] = "Only Qualified leads can be converted to customers.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            var converted = await _svc.ConvertAsync(id, user!.Id);

            if (converted)
            {
                await _audit.LogAsync(user.Id, user.Email ?? "", "Convert", "Lead",
                    id.ToString(), oldValue: "Qualified", newValue: "Converted",
                    ipAddress: ip);
                TempData["Success"] = "Lead converted to customer successfully.";
            }
            else
            {
                TempData["Error"] = "Conversion failed. Lead may no longer be qualified.";
            }

            return RedirectToAction(nameof(Index));
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
