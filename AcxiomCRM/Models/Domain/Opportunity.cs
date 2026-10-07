using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcxiomCRM.Models.Domain
{
    public class Opportunity
    {
        public int OpportunityId { get; set; }

        [Required, StringLength(150)]
        public string OpportunityName { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? LeadId { get; set; }
        public Lead? Lead { get; set; }

        [Required, StringLength(30)]
        public string Stage { get; set; } = "Qualification";

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999999.99, ErrorMessage = "Opportunity Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public int Probability { get; set; }

        public DateTime ExpectedCloseDate { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Open";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? AssignedTo { get; set; }

        [NotMapped]
        public decimal WeightedAmount => Amount * Probability / 100;

        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}
