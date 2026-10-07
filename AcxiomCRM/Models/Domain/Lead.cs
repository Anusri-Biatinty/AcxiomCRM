using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models.Domain
{
    public class Lead
    {
        public int LeadId { get; set; }

        [Required, StringLength(10)]
        public string LeadCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LeadName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Source { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Status { get; set; } = "New";

        [Required, StringLength(20)]
        public string Priority { get; set; } = "Medium";

        [Range(0, 999999999.99)]
        public decimal ExpectedValue { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? AssignedTo { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}
