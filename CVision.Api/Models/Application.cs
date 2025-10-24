using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Job")]
        public int JobId { get; set; }
        public Job Job { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("CV")]
        public int CvId { get; set; }
        public CV CV { get; set; }

        public string Status { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}