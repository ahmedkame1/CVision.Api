// Models/Education.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class Education
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CV")]
        public int CvId { get; set; }
        public CV CV { get; set; } = null!;

        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CurrentlyStudying { get; set; } // استخدام CurrentlyStudying بدل IsCurrent
        public string? Grade { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}