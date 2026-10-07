using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.DTOs
{
    // ── Customer ──────────────────────────────────────────────────────────────
    public class CustomerDto
    {
        public int    CustomerId   { get; set; }
        public string CustomerCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email        { get; set; } = string.Empty;

        [Required, RegularExpression(@"^[6-9]\d{9}$")]
        public string Phone        { get; set; } = string.Empty;

        public string CompanyName  { get; set; } = string.Empty;
        public string Status       { get; set; } = "Active";
        public DateTime CreatedDate { get; set; }
    }

    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email        { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit phone number.")]
        public string Phone        { get; set; } = string.Empty;

        public string CompanyName  { get; set; } = string.Empty;
        public string Status       { get; set; } = "Active";
    }

    // ── Lead ─────────────────────────────────────────────────────────────────
    public class LeadDto
    {
        public int    LeadId       { get; set; }
        public string LeadCode     { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lead name is required.")]
        [StringLength(100)]
        public string LeadName     { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email        { get; set; } = string.Empty;

        [Required, RegularExpression(@"^[6-9]\d{9}$")]
        public string Phone        { get; set; } = string.Empty;

        public string Source       { get; set; } = string.Empty;
        public string Status       { get; set; } = "New";
        public string Priority     { get; set; } = "Medium";

        [Range(0, 999999999.99)]
        public decimal ExpectedValue { get; set; }
        public DateTime CreatedDate  { get; set; }
    }

    // ── Opportunity ───────────────────────────────────────────────────────────
    public class OpportunityDto
    {
        public int    OpportunityId   { get; set; }

        [Required]
        public string OpportunityName { get; set; } = string.Empty;

        [Required]
        public int    CustomerId      { get; set; }

        [Required]
        public string Stage           { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount         { get; set; }

        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public int    Probability     { get; set; }

        public DateTime ExpectedCloseDate { get; set; }
        public string   Status        { get; set; } = string.Empty;
        public decimal  WeightedAmount { get; set; }
    }

    // ── API Error ─────────────────────────────────────────────────────────────
    public class ApiError
    {
        public string  Message { get; set; } = string.Empty;
        public string? Field   { get; set; }
    }
}
