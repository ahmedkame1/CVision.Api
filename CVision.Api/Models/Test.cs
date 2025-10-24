using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CVision.Api.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; } // in minutes
        public int TotalMarks { get; set; }

        public ICollection<TestResult> TestResults { get; set; }
    }
}
