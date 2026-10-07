using AcxiomCRM.Models.Domain;
using AcxiomCRM.ViewModels.Customer;
using AcxiomCRM.ViewModels.Lead;
using AcxiomCRM.ViewModels.Opportunity;
using AcxiomCRM.ViewModels.FollowUp;

namespace AcxiomCRM.Services
{
    // ── Audit ─────────────────────────────────────────────────────────────────
    public interface IAuditService
    {
        Task LogAsync(string userId, string userName, string action,
            string entityName, string? recordId = null,
            string? oldValue = null, string? newValue = null,
            string? ipAddress = null, string result = "Success");
    }

    // ── Customer ──────────────────────────────────────────────────────────────
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync(string? userId = null, bool isPrivileged = false);
        Task<Customer?> GetByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);
        Task<Customer> CreateAsync(CustomerViewModel vm, string createdBy);
        Task<bool> UpdateAsync(CustomerViewModel vm);
        Task<bool> DeactivateAsync(int id);
        Task<string> GenerateCodeAsync();
    }

    // ── Lead ─────────────────────────────────────────────────────────────────
    public interface ILeadService
    {
        Task<List<Lead>> GetAllAsync(string? userId = null, bool isPrivileged = false);
        Task<Lead?> GetByIdAsync(int id);
        Task<Lead> CreateAsync(LeadViewModel vm, string createdBy);
        Task<bool> UpdateAsync(LeadViewModel vm);
        Task<bool> ConvertAsync(int leadId, string convertedBy);
        Task<string> GenerateCodeAsync();
    }

    // ── Opportunity ───────────────────────────────────────────────────────────
    public interface IOpportunityService
    {
        Task<List<Opportunity>> GetAllAsync(string? userId = null, bool isPrivileged = false);
        Task<Opportunity?> GetByIdAsync(int id);
        Task<Opportunity> CreateAsync(OpportunityViewModel vm, string createdBy);
        Task<bool> UpdateAsync(OpportunityViewModel vm);
        Task<bool> DeleteAsync(int id);
    }

    // ── FollowUp ──────────────────────────────────────────────────────────────
    public interface IFollowUpService
    {
        Task<List<FollowUp>> GetAllAsync(string? userId = null, bool isPrivileged = false);
        Task<FollowUp?> GetByIdAsync(int id);
        Task<FollowUp> CreateAsync(FollowUpViewModel vm, string createdBy);
        Task<bool> CompleteAsync(int id);
        Task<bool> CancelAsync(int id);
    }
}
