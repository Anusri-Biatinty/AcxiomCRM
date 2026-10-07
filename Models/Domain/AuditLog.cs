using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models.Domain
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        [Required, StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string EntityName { get; set; } = string.Empty;

        public string? RecordId { get; set; }

        [StringLength(2000)]
        public string? OldValue { get; set; }

        [StringLength(2000)]
        public string? NewValue { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(20)]
        public string Result { get; set; } = "Success";
    }
}
