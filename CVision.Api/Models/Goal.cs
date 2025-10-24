using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CVision.Api.Models
{
    public class Goal
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<UserGoal> UserGoals { get; set; }
    }
}
