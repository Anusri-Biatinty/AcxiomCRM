using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.ViewModels.Opportunity
{
    public class OpportunityViewModel
    {
        public int OpportunityId { get; set; }

        [Required(ErrorMessage = "Opportunity name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Opportunity Name")]
        public string OpportunityName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer is required.")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Source Lead")]
        public int? LeadId { get; set; }

        [Required(ErrorMessage = "Stage is required.")]
        public string Stage { get; set; } = "Qualification";

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 999999999.99,
            ErrorMessage = "Opportunity Amount must be greater than 0.")]
        [Display(Name = "Amount (₹)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Probability is required.")]
        [Range(0, 100,
            ErrorMessage = "Probability must be between 0 and 100.")]
        [Display(Name = "Probability (%)")]
        public int Probability { get; set; }

        [Required(ErrorMessage = "Expected close date is required.")]
        [Display(Name = "Expected Close Date")]
        [DataType(DataType.Date)]
        public DateTime ExpectedCloseDate { get; set; } = DateTime.Today.AddDays(30);

        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        public string Status { get; set; } = "Open";

        // Dropdown data
        public List<SelectListItem> Customers  { get; set; } = new();
        public List<SelectListItem> SalesUsers { get; set; } = new();
    }
}
