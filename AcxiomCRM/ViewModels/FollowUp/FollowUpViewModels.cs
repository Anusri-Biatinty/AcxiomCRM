using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.ViewModels.FollowUp
{
    public class FollowUpViewModel
    {
        public int FollowUpId { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        [Display(Name = "Opportunity")]
        public int? OpportunityId { get; set; }

        [Required(ErrorMessage = "Follow-up date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Follow-Up Date")]
        public DateTime FollowUpDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Type is required.")]
        [Display(Name = "Follow-Up Type")]
        public string FollowUpType { get; set; } = "Call";

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
        public string Subject { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public string Status { get; set; } = "Planned";

        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        // Dropdown data
        public List<SelectListItem> Customers     { get; set; } = new();
        public List<SelectListItem> Leads         { get; set; } = new();
        public List<SelectListItem> Opportunities { get; set; } = new();
        public List<SelectListItem> SalesUsers    { get; set; } = new();
    }
}
