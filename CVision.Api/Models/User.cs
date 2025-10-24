using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [ForeignKey("Hr")]
        public int HrId { get; set; }
        public Hr Hr { get; set; } = null!;

        // Match percentage from ATS (to be set later by HR)
        public double? MatchPercentage { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Pending;
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public DateTime? MatchPercentageUpdatedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual ICollection<CV> CVs { get; set; } = new List<CV>();
    }

    public enum UserStatus
    {
        Pending,        // معلق
        Qualified,      // مؤهل
        NotQualified,   // غير مؤهل
        Contacted,      // تم التواصل
        Hired           // تم التعيين
    }
}