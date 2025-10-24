using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class CV
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsPrimary { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public ICollection<CvAttachment> Attachments { get; set; } = new List<CvAttachment>();
    }
}