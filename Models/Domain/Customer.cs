using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models.Domain
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required, StringLength(10)]
        public string CustomerCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? AssignedTo { get; set; }

        public ICollection<Lead>        Leads         { get; set; } = new List<Lead>();
        public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
        public ICollection<FollowUp>    FollowUps     { get; set; } = new List<FollowUp>();
        public ICollection<Activity>    Activities    { get; set; } = new List<Activity>();
    }
}
