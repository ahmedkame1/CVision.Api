using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Test")]
        public int TestId { get; set; }
        public Test Test { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public double Score { get; set; }
        public DateTime TakenAt { get; set; } = DateTime.Now;
        public string? DetailsJson { get; set; }
    }
}