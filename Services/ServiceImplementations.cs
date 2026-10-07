using AcxiomCRM.Data;
using AcxiomCRM.Models.Domain;
using AcxiomCRM.ViewModels.Customer;
using AcxiomCRM.ViewModels.Lead;
using AcxiomCRM.ViewModels.Opportunity;
using AcxiomCRM.ViewModels.FollowUp;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    // ── Audit Service ─────────────────────────────────────────────────────────
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;
        public AuditService(ApplicationDbContext db) { _db = db; }

        public async Task LogAsync(string userId, string userName, string action,
            string entityName, string? recordId = null,
            string? oldValue = null, string? newValue = null,
            string? ipAddress = null, string result = "Success")
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId      = userId,
                UserName    = userName,
                Action      = action,
                EntityName  = entityName,
                RecordId    = recordId,
                OldValue    = oldValue,
                NewValue    = newValue,
                IpAddress   = ipAddress,
                Result      = result,
                CreatedDate = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }

    // ── Customer Service ──────────────────────────────────────────────────────
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _db;
        public CustomerService(ApplicationDbContext db) { _db = db; }

        public async Task<List<Customer>> GetAllAsync(string? userId = null, bool isPrivileged = false)
        {
            var q = _db.Customers.AsQueryable();
            if (!isPrivileged && userId != null)
                q = q.Where(c => c.AssignedTo == userId || c.CreatedBy == userId);
            return await q.OrderByDescending(c => c.CreatedDate).ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id) => await _db.Customers.FindAsync(id);

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var q = _db.Customers.Where(c => c.Email.ToLower() == email.ToLower());
            if (excludeId.HasValue) q = q.Where(c => c.CustomerId != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
        {
            var q = _db.Customers.Where(c => c.Phone == phone);
            if (excludeId.HasValue) q = q.Where(c => c.CustomerId != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task<Customer> CreateAsync(CustomerViewModel vm, string createdBy)
        {
            var c = new Customer
            {
                CustomerCode = await GenerateCodeAsync(),
                CustomerName = vm.CustomerName,
                Email        = vm.Email,
                Phone        = vm.Phone,
                CompanyName  = vm.CompanyName,
                Address      = vm.Address,
                City         = vm.City,
                State        = vm.State,
                Status       = vm.Status,
                AssignedTo   = vm.AssignedTo,
                CreatedBy    = createdBy,
                CreatedDate  = DateTime.UtcNow
            };
            _db.Customers.Add(c);
            await _db.SaveChangesAsync();
            return c;
        }

        public async Task<bool> UpdateAsync(CustomerViewModel vm)
        {
            var c = await _db.Customers.FindAsync(vm.CustomerId);
            if (c == null) return false;
            c.CustomerName = vm.CustomerName;
            c.Email        = vm.Email;
            c.Phone        = vm.Phone;
            c.CompanyName  = vm.CompanyName;
            c.Address      = vm.Address;
            c.City         = vm.City;
            c.State        = vm.State;
            c.Status       = vm.Status;
            c.AssignedTo   = vm.AssignedTo;
            c.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var c = await _db.Customers.FindAsync(id);
            if (c == null) return false;
            c.Status       = "Inactive";
            c.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateCodeAsync()
        {
            var count = await _db.Customers.CountAsync();
            return $"CUST{(count + 1):D4}";
        }
    }

    // ── Lead Service ──────────────────────────────────────────────────────────
    public class LeadService : ILeadService
    {
        private readonly ApplicationDbContext _db;
        public LeadService(ApplicationDbContext db) { _db = db; }

        public async Task<List<Lead>> GetAllAsync(string? userId = null, bool isPrivileged = false)
        {
            var q = _db.Leads.AsQueryable();
            if (!isPrivileged && userId != null)
                q = q.Where(l => l.AssignedTo == userId || l.CreatedBy == userId);
            return await q.OrderByDescending(l => l.CreatedDate).ToListAsync();
        }

        public async Task<Lead?> GetByIdAsync(int id) => await _db.Leads.FindAsync(id);

        public async Task<Lead> CreateAsync(LeadViewModel vm, string createdBy)
        {
            var l = new Lead
            {
                LeadCode      = await GenerateCodeAsync(),
                LeadName      = vm.LeadName,
                Email         = vm.Email,
                Phone         = vm.Phone,
                CompanyName   = vm.CompanyName,
                Source        = vm.Source,
                Status        = vm.Status,
                Priority      = vm.Priority,
                ExpectedValue = vm.ExpectedValue,
                AssignedTo    = vm.AssignedTo,
                CreatedBy     = createdBy,
                CreatedDate   = DateTime.UtcNow
            };
            _db.Leads.Add(l);
            await _db.SaveChangesAsync();
            return l;
        }

        public async Task<bool> UpdateAsync(LeadViewModel vm)
        {
            var l = await _db.Leads.FindAsync(vm.LeadId);
            if (l == null) return false;
            l.LeadName      = vm.LeadName;
            l.Email         = vm.Email;
            l.Phone         = vm.Phone;
            l.CompanyName   = vm.CompanyName;
            l.Source        = vm.Source;
            l.Status        = vm.Status;
            l.Priority      = vm.Priority;
            l.ExpectedValue = vm.ExpectedValue;
            l.AssignedTo    = vm.AssignedTo;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConvertAsync(int leadId, string convertedBy)
        {
            var lead = await _db.Leads.FindAsync(leadId);
            if (lead == null || lead.Status != "Qualified") return false;

            // Create customer from lead
            var count    = await _db.Customers.CountAsync();
            var customer = new Customer
            {
                CustomerCode = $"CUST{(count + 1):D4}",
                CustomerName = lead.LeadName,
                Email        = lead.Email,
                Phone        = lead.Phone,
                CompanyName  = lead.CompanyName,
                Status       = "Active",
                AssignedTo   = lead.AssignedTo,
                CreatedBy    = convertedBy,
                CreatedDate  = DateTime.UtcNow
            };
            _db.Customers.Add(customer);

            lead.Status     = "Converted";
            lead.CustomerId = customer.CustomerId;
            await _db.SaveChangesAsync();

            // Update lead with real customer id
            lead.CustomerId = customer.CustomerId;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateCodeAsync()
        {
            var count = await _db.Leads.CountAsync();
            return $"LEAD{(count + 1):D4}";
        }
    }

    // ── Opportunity Service ───────────────────────────────────────────────────
    public class OpportunityService : IOpportunityService
    {
        private readonly ApplicationDbContext _db;
        public OpportunityService(ApplicationDbContext db) { _db = db; }

        public async Task<List<Opportunity>> GetAllAsync(string? userId = null, bool isPrivileged = false)
        {
            var q = _db.Opportunities.Include(o => o.Customer).AsQueryable();
            if (!isPrivileged && userId != null)
                q = q.Where(o => o.AssignedTo == userId || o.CreatedBy == userId);
            return await q.OrderByDescending(o => o.CreatedDate).ToListAsync();
        }

        public async Task<Opportunity?> GetByIdAsync(int id)
            => await _db.Opportunities.Include(o => o.Customer).FirstOrDefaultAsync(o => o.OpportunityId == id);

        public async Task<Opportunity> CreateAsync(OpportunityViewModel vm, string createdBy)
        {
            var o = new Opportunity
            {
                OpportunityName  = vm.OpportunityName,
                CustomerId       = vm.CustomerId,
                LeadId           = vm.LeadId,
                Stage            = vm.Stage,
                Amount           = vm.Amount,
                Probability      = vm.Probability,
                ExpectedCloseDate = vm.ExpectedCloseDate,
                Notes            = vm.Notes,
                Status           = vm.Status,
                AssignedTo       = vm.AssignedTo,
                CreatedBy        = createdBy,
                CreatedDate      = DateTime.UtcNow
            };
            _db.Opportunities.Add(o);
            await _db.SaveChangesAsync();
            return o;
        }

        public async Task<bool> UpdateAsync(OpportunityViewModel vm)
        {
            var o = await _db.Opportunities.FindAsync(vm.OpportunityId);
            if (o == null) return false;
            o.OpportunityName   = vm.OpportunityName;
            o.CustomerId        = vm.CustomerId;
            o.Stage             = vm.Stage;
            o.Amount            = vm.Amount;
            o.Probability       = vm.Probability;
            o.ExpectedCloseDate = vm.ExpectedCloseDate;
            o.Notes             = vm.Notes;
            o.Status            = vm.Status;
            o.AssignedTo        = vm.AssignedTo;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var o = await _db.Opportunities.FindAsync(id);
            if (o == null) return false;
            _db.Opportunities.Remove(o);
            await _db.SaveChangesAsync();
            return true;
        }
    }

    // ── FollowUp Service ──────────────────────────────────────────────────────
    public class FollowUpService : IFollowUpService
    {
        private readonly ApplicationDbContext _db;
        public FollowUpService(ApplicationDbContext db) { _db = db; }

        public async Task<List<FollowUp>> GetAllAsync(string? userId = null, bool isPrivileged = false)
        {
            var q = _db.FollowUps
                .Include(f => f.Customer)
                .Include(f => f.Lead)
                .Include(f => f.Opportunity)
                .AsQueryable();
            if (!isPrivileged && userId != null)
                q = q.Where(f => f.AssignedTo == userId || f.CreatedBy == userId);
            return await q.OrderByDescending(f => f.FollowUpDate).ToListAsync();
        }

        public async Task<FollowUp?> GetByIdAsync(int id) => await _db.FollowUps.FindAsync(id);

        public async Task<FollowUp> CreateAsync(FollowUpViewModel vm, string createdBy)
        {
            var f = new FollowUp
            {
                CustomerId    = vm.CustomerId,
                LeadId        = vm.LeadId,
                OpportunityId = vm.OpportunityId,
                FollowUpDate  = vm.FollowUpDate,
                FollowUpType  = vm.FollowUpType,
                Subject       = vm.Subject,
                Remarks       = vm.Remarks,
                Status        = vm.Status,
                AssignedTo    = vm.AssignedTo,
                CreatedBy     = createdBy,
                CreatedDate   = DateTime.UtcNow
            };
            _db.FollowUps.Add(f);
            await _db.SaveChangesAsync();
            return f;
        }

        public async Task<bool> CompleteAsync(int id)
        {
            var f = await _db.FollowUps.FindAsync(id);
            if (f == null) return false;
            f.Status = "Completed";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id)
        {
            var f = await _db.FollowUps.FindAsync(id);
            if (f == null) return false;
            f.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
