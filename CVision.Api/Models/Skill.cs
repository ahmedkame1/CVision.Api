// Models/Skill.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class Skill
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CV")]
        public int CvId { get; set; }
        public CV CV { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = "Intermediate"; // Beginner, Intermediate, Advanced, Expert
        public string Category { get; set; } = "Technical"; // Technical, Soft, Language, etc.
        public int? YearsOfExperience { get; set; }
        public int DisplayOrder { get; set; }
    }
}