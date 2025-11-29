// Models/Certification.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class Certification
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CV")]
        public int CvId { get; set; }
        public CV CV { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? CredentialId { get; set; }
        public string? CredentialUrl { get; set; }
        public int DisplayOrder { get; set; }
    }
}