using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace CVision.Api.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Hr")]
        public int HrId { get; set; }
        public Hr Hr { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string SalaryRange { get; set; }
        public string EmploymentType { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.Now;
        public DateTime? ClosesAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Application> Applications { get; set; }
    }
}
