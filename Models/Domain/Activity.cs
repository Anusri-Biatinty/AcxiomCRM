using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models.Domain
{
    public class Activity
    {
        public int ActivityId { get; set; }

        [Required, StringLength(20)]
        public string ActivityType { get; set; } = "Call";

        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime ActivityDate { get; set; }

        public int? CustomerId  { get; set; }
        public Customer? Customer { get; set; }

        public int? LeadId { get; set; }
        public Lead? Lead  { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pending";

        public string? AssignedTo { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string   CreatedBy   { get; set; } = string.Empty;
    }
}
