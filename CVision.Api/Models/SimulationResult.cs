using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class SimulationResult
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Name { get; set; }
        public string ResultJson { get; set; }
        public DateTime TakenAt { get; set; } = DateTime.Now;
    }
}