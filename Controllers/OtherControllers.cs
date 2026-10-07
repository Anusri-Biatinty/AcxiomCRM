using AcxiomCRM.Data;
using AcxiomCRM.Models.Identity;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels.FollowUp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// ═══════════════════════════════════════════════════════════════════════════════
// FOLLOW-UP CONTROLLER
// ═══════════════════════════════════════════════════════════════════════════════
namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class FollowUpController : Controller
    {
        private readonly IFollowUpService              _svc;
        private readonly ICustomerService              _custSvc;
        private readonly ILeadService                  _leadSvc;
        private readonly IAuditService                 _audit;
        private readonly UserManager<ApplicationUser> _users;
        private readonly ApplicationDbContext          _db;

        public FollowUpController(IFollowUpService svc, ICustomerService custSvc,
            ILeadService leadSvc, IAuditService audit,
            UserManager<ApplicationUser> users, ApplicationDbContext db)
        {
            _svc     = svc;
            _custSvc = custSvc;
            _leadSvc = leadSvc;
            _audit   = audit;
            _users   = users;
            _db      = db;
        }

        public async Task<IActionResult> Index(string? status, DateTime? date)
        {
            var user  = await _users.GetUserAsync(User);
            var roles = await _users.GetRolesAsync(user!);
            bool isPriv = roles.Contains("Admin") || roles.Contains("Manager");

            var list = await _svc.GetAllAsync(user!.Id, isPriv);

            if (!string.IsNullOrWhiteSpace(status))
                list = list.Where(f => f.Status == status).ToList();
            if (date.HasValue)
                list = list.Where(f => f.FollowUpDate.Date == date.Value.Date).ToList();

            ViewBag.Status = status;
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new FollowUpViewModel
            {
                FollowUpDate  = DateTime.Today,
                Customers     = await GetCustomerList(),
                Leads         = await GetLeadList(),
                Opportunities = await GetOpportunityList(),
                SalesUsers    = await GetSalesUserList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FollowUpViewModel model)
        {
            // Business rule: follow-up date cannot be earlier than today
            if (model.FollowUpDate.Date < DateTime.Today)
                ModelState.AddModelError("FollowUpDate",
                    "Follow-up date cannot be earlier than today.");

            if (!ModelState.IsValid)
            {
                model.Customers     = await GetCustomerList();
                model.Leads         = await GetLeadList();
                model.Opportunities = await GetOpportunityList();
                model.SalesUsers    = await GetSalesUserList();
                return View(model);
            }

            var user   = await _users.GetUserAsync(User);
            if (string.IsNullOrEmpty(model.AssignedTo)) model.AssignedTo = user!.Id;

            var followUp = await _svc.CreateAsync(model, user!.Id);
            var ip       = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _audit.LogAsync(user.Id, user.Email ?? "", "Create", "FollowUp",
                followUp.FollowUpId.ToString(), ipAddress: ip);

            TempData["Success"] = "Follow-up scheduled successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _svc.CompleteAsync(id);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Update", "FollowUp",
                id.ToString(), oldValue: "Planned", newValue: "Completed", ipAddress: ip);

            TempData["Success"] = "Follow-up marked as completed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _svc.CancelAsync(id);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Update", "FollowUp",
                id.ToString(), oldValue: "Planned", newValue: "Cancelled", ipAddress: ip);

            TempData["Success"] = "Follow-up cancelled.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetCustomerList()
        {
            var customers = await _custSvc.GetAllAsync(isPrivileged: true);
            return customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(), Text = c.CustomerName
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetLeadList()
        {
            var leads = await _leadSvc.GetAllAsync(isPrivileged: true);
            return leads.Select(l => new SelectListItem
            {
                Value = l.LeadId.ToString(), Text = l.LeadName
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetOpportunityList()
            => await _db.Opportunities
                .Select(o => new SelectListItem
                {
                    Value = o.OpportunityId.ToString(), Text = o.OpportunityName
                }).ToListAsync();

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

// ═══════════════════════════════════════════════════════════════════════════════
// AUDIT LOG CONTROLLER
// ═══════════════════════════════════════════════════════════════════════════════
namespace AcxiomCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AuditLogController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> Index(string? action, string? entity,
            string? user, DateTime? fromDate, DateTime? toDate)
        {
            var q = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(action))
                q = q.Where(a => a.Action == action);
            if (!string.IsNullOrWhiteSpace(entity))
                q = q.Where(a => a.EntityName == entity);
            if (!string.IsNullOrWhiteSpace(user))
                q = q.Where(a => a.UserName != null &&
                    a.UserName.Contains(user, StringComparison.OrdinalIgnoreCase));
            if (fromDate.HasValue)
                q = q.Where(a => a.CreatedDate >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(a => a.CreatedDate <= toDate.Value.AddDays(1));

            ViewBag.Action   = action;
            ViewBag.Entity   = entity;
            ViewBag.User     = user;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate   = toDate?.ToString("yyyy-MM-dd");

            return View(await q.OrderByDescending(a => a.CreatedDate).Take(500).ToListAsync());
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// USER MANAGEMENT CONTROLLER
// ═══════════════════════════════════════════════════════════════════════════════
namespace AcxiomCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser>  _users;
        private readonly RoleManager<IdentityRole>    _roles;
        private readonly IAuditService                 _audit;

        public UserManagementController(UserManager<ApplicationUser> users,
            RoleManager<IdentityRole> roles, IAuditService audit)
        {
            _users = users;
            _roles = roles;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var users    = _users.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var u in users)
                userRoles[u.Id] = await _users.GetRolesAsync(u);

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string userId)
        {
            var target = await _users.FindByIdAsync(userId);
            if (target == null) return NotFound();

            target.IsActive = !target.IsActive;
            await _users.UpdateAsync(target);

            var admin = await _users.GetUserAsync(User);
            await _audit.LogAsync(admin!.Id, admin.Email ?? "", "UpdateUser", "User",
                userId, newValue: $"IsActive={target.IsActive}",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = $"User {(target.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var target = await _users.FindByIdAsync(userId);
            if (target == null) return NotFound();

            var currentRoles = await _users.GetRolesAsync(target);
            await _users.RemoveFromRolesAsync(target, currentRoles);
            await _users.AddToRoleAsync(target, newRole);

            var admin = await _users.GetUserAsync(User);
            await _audit.LogAsync(admin!.Id, admin.Email ?? "", "RoleChange", "User",
                userId,
                oldValue: string.Join(",", currentRoles),
                newValue: newRole,
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "User role updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                TempData["Error"] = "Password must be at least 8 characters.";
                return RedirectToAction(nameof(Index));
            }

            var target = await _users.FindByIdAsync(userId);
            if (target == null) return NotFound();

            var token  = await _users.GeneratePasswordResetTokenAsync(target);
            var result = await _users.ResetPasswordAsync(target, token, newPassword);

            var admin = await _users.GetUserAsync(User);
            if (result.Succeeded)
            {
                await _audit.LogAsync(admin!.Id, admin.Email ?? "",
                    "PasswordReset", "User", userId,
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = "Password reset successfully.";
            }
            else
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockAccount(string userId)
        {
            var target = await _users.FindByIdAsync(userId);
            if (target == null) return NotFound();

            await _users.ResetAccessFailedCountAsync(target);
            await _users.SetLockoutEndDateAsync(target, null);

            var admin = await _users.GetUserAsync(User);
            await _audit.LogAsync(admin!.Id, admin.Email ?? string.Empty, "UnlockAccount", "User",
                userId, ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Account unlocked.";
            return RedirectToAction(nameof(Index));
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// REPORTS CONTROLLER
// ═══════════════════════════════════════════════════════════════════════════════
namespace AcxiomCRM.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext          _db;
        private readonly UserManager<ApplicationUser> _users;

        public ReportsController(ApplicationDbContext db,
            UserManager<ApplicationUser> users)
        {
            _db    = db;
            _users = users;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> CustomerReport(string? status)
        {
            var q = _db.Customers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(c => c.Status == status);
            ViewBag.Status = status;
            return View(await q.OrderBy(c => c.CustomerName).ToListAsync());
        }

        public async Task<IActionResult> LeadReport(string? status, string? source)
        {
            var q = _db.Leads.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(l => l.Status == status);
            if (!string.IsNullOrWhiteSpace(source)) q = q.Where(l => l.Source == source);
            ViewBag.Status = status;
            ViewBag.Source = source;
            return View(await q.OrderByDescending(l => l.CreatedDate).ToListAsync());
        }

        public async Task<IActionResult> OpportunityReport(string? stage)
        {
            var q = _db.Opportunities.Include(o => o.Customer).AsQueryable();
            if (!string.IsNullOrWhiteSpace(stage)) q = q.Where(o => o.Stage == stage);
            ViewBag.Stage = stage;
            return View(await q.OrderByDescending(o => o.Amount).ToListAsync());
        }

        public async Task<IActionResult> PipelineReport()
        {
            var pipeline = await _db.Opportunities
                .Where(o => o.Stage != "Won" && o.Stage != "Lost")
                .Include(o => o.Customer)
                .GroupBy(o => o.Stage)
                .Select(g => new
                {
                    Stage          = g.Key,
                    Count          = g.Count(),
                    TotalAmount    = g.Sum(o => o.Amount),
                    WeightedAmount = g.Sum(o => o.Amount * o.Probability / 100)
                })
                .ToListAsync();

            return View(pipeline);
        }

        public async Task<IActionResult> FollowUpReport(string? status)
        {
            var q = _db.FollowUps
                .Include(f => f.Customer)
                .Include(f => f.Lead)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(f => f.Status == status);

            ViewBag.Status       = status;
            ViewBag.OverdueCount = await _db.FollowUps.CountAsync(f =>
                f.Status == "Planned" && f.FollowUpDate < DateTime.Today);

            return View(await q.OrderByDescending(f => f.FollowUpDate).ToListAsync());
        }

        public async Task<IActionResult> AuditReport(string? action, DateTime? from, DateTime? to)
        {
            var q = _db.AuditLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(action)) q = q.Where(a => a.Action == action);
            if (from.HasValue) q = q.Where(a => a.CreatedDate >= from.Value);
            if (to.HasValue)   q = q.Where(a => a.CreatedDate <= to.Value.AddDays(1));
            return View(await q.OrderByDescending(a => a.CreatedDate).Take(300).ToListAsync());
        }

        public async Task<IActionResult> UserActivityReport()
        {
            var activity = await _db.AuditLogs
                .GroupBy(a => new { a.UserId, a.UserName })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.UserName,
                    TotalActions = g.Count(),
                    LastActivity = g.Max(a => a.CreatedDate)
                })
                .OrderByDescending(x => x.TotalActions)
                .ToListAsync();

            return View(activity);
        }
    }
}
