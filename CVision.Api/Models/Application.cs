// Models/Application.cs
using CVision.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Application
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Job")]
    public int JobId { get; set; }
    public Job? Job { get; set; } // جعلها nullable

    [ForeignKey("User")]
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; } // جعلها nullable

    [ForeignKey("CV")]
    public int CvId { get; set; }
    public CV? CV { get; set; } // جعلها nullable

    public string Status { get; set; } = "Applied"; // قيمة افتراضية
    public DateTime AppliedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}