using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models.Domain
{
    public class FollowUp
    {
        public int FollowUpId { get; set; }

        public int? CustomerId   { get; set; }
        public Customer? Customer { get; set; }

        public int? LeadId { get; set; }
        public Lead? Lead  { get; set; }

        public int? OpportunityId  { get; set; }
        public Opportunity? Opportunity { get; set; }

        [Required]
        public DateTime FollowUpDate { get; set; }

        [Required, StringLength(30)]
        public string FollowUpType { get; set; } = "Call";

        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Planned";

        public string? AssignedTo { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string   CreatedBy   { get; set; } = string.Empty;
    }
}
