using AcxiomCRM.Data;
using AcxiomCRM.Models.Identity;
using AcxiomCRM.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext          _db;
        private readonly UserManager<ApplicationUser> _users;

        public DashboardController(ApplicationDbContext db,
            UserManager<ApplicationUser> users)
        {
            _db    = db;
            _users = users;
        }

        public async Task<IActionResult> Index()
        {
            var user       = await _users.GetUserAsync(User);
            var roles      = await _users.GetRolesAsync(user!);
            bool isPriv    = roles.Contains("Admin") || roles.Contains("Manager");
            string userId  = user!.Id;

            // ── Scoped queries ────────────────────────────────────────────────
            var custQ   = _db.Customers.AsQueryable();
            var leadQ   = _db.Leads.AsQueryable();
            var oppQ    = _db.Opportunities.AsQueryable();
            var followQ = _db.FollowUps.AsQueryable();

            if (!isPriv)
            {
                custQ   = custQ.Where(c => c.AssignedTo == userId || c.CreatedBy == userId);
                leadQ   = leadQ.Where(l => l.AssignedTo == userId || l.CreatedBy == userId);
                oppQ    = oppQ.Where(o => o.AssignedTo == userId || o.CreatedBy == userId);
                followQ = followQ.Where(f => f.AssignedTo == userId || f.CreatedBy == userId);
            }

            // ── Lead status chart data ────────────────────────────────────────
            var leadGroups = await leadQ
                .GroupBy(l => l.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // ── Opportunity pipeline chart data ───────────────────────────────
            var oppGroups = await oppQ
                .GroupBy(o => o.Stage)
                .Select(g => new { Stage = g.Key, Total = g.Sum(o => o.Amount) })
                .ToListAsync();

            // ── Monthly sales (last 6 months) ─────────────────────────────────
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyWon = await oppQ
                .Where(o => o.Stage == "Won" && o.CreatedDate >= sixMonthsAgo)
                .GroupBy(o => new { o.CreatedDate.Year, o.CreatedDate.Month })
                .Select(g => new
                {
                    Label  = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                    Amount = g.Sum(o => o.Amount)
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalCustomers     = await custQ.CountAsync(),
                TotalLeads         = await leadQ.CountAsync(),
                OpenLeads          = await leadQ.CountAsync(l =>
                    l.Status != "Converted" && l.Status != "Lost" && l.Status != "Unqualified"),
                TotalOpportunities = await oppQ.CountAsync(),
                OpenOpportunities  = await oppQ.CountAsync(o =>
                    o.Stage != "Won" && o.Stage != "Lost"),
                WonOpportunities   = await oppQ.CountAsync(o => o.Stage == "Won"),
                LostOpportunities  = await oppQ.CountAsync(o => o.Stage == "Lost"),
                TotalPipelineValue = await oppQ
                    .Where(o => o.Stage != "Won" && o.Stage != "Lost")
                    .SumAsync(o => (decimal?)o.Amount) ?? 0,
                PendingFollowUps   = await followQ.CountAsync(f => f.Status == "Planned"),
                OverdueFollowUps   = await followQ.CountAsync(f =>
                    f.Status == "Planned" && f.FollowUpDate < DateTime.Today),

                LeadStatusJson = JsonSerializer.Serialize(new
                {
                    labels = leadGroups.Select(g => g.Status).ToArray(),
                    data   = leadGroups.Select(g => g.Count).ToArray()
                }),

                PipelineJson = JsonSerializer.Serialize(new
                {
                    labels = oppGroups.Select(g => g.Stage).ToArray(),
                    data   = oppGroups.Select(g => g.Total).ToArray()
                }),

                MonthlySalesJson = JsonSerializer.Serialize(new
                {
                    labels = monthlyWon.Select(m => m.Label).ToArray(),
                    data   = monthlyWon.Select(m => m.Amount).ToArray()
                })
            };

            return View(vm);
        }
    }
}
