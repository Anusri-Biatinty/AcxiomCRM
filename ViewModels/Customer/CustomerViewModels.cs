using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.ViewModels.Customer
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }

        [Display(Name = "Customer Code")]
        public string CustomerCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^[6-9]\d{9}$",
            ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Active";

        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        public List<SelectListItem> SalesUsers { get; set; } = new();
    }

    public class CustomerSearchViewModel
    {
        public string? Name    { get; set; }
        public string? Email   { get; set; }
        public string? Phone   { get; set; }
        public string? Company { get; set; }
        public string? Status  { get; set; }

        public List<AcxiomCRM.Models.Domain.Customer> Results { get; set; } = new();
    }
}
