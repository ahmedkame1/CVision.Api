// Models/PersonalInfo.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class PersonalInfo
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CV")]
        public int CvId { get; set; }
        public CV CV { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // استخدام Location بدل Address
        public string? LinkedIn { get; set; } // استخدام LinkedIn بدل LinkedInUrl
        public string? GitHub { get; set; } // استخدام GitHub بدل GitHubUrl
        public string? Website { get; set; }
    }
}