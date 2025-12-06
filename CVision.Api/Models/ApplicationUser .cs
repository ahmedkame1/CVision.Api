// Models/ApplicationUser.cs
using CVision.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ApplicationUser : IdentityUser<int>
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Email confirmation
    public new bool EmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmationTokenExpiry { get; set; }

    // Navigation properties
    public Hr? HrProfile { get; set; }
    public ICollection<CV> CVs { get; set; } = new List<CV>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    public ICollection<SimulationResult> SimulationResults { get; set; } = new List<SimulationResult>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<UserGoal> UserGoals { get; set; } = new List<UserGoal>();
}