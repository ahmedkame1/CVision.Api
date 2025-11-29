// Models/Experience.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class Experience
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CV")]
        public int CvId { get; set; }
        public CV CV { get; set; } = null!;

        public string JobTitle { get; set; } = string.Empty; // استخدام JobTitle بدل Position
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CurrentlyWorking { get; set; } // استخدام CurrentlyWorking بدل IsCurrent
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}