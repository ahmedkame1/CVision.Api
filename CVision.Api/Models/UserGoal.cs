using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class UserGoal
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("Goal")]
        public int GoalId { get; set; }
        public Goal Goal { get; set; }

        public string Status { get; set; }
        public int Progress { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}