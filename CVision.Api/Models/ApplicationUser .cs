// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CVision.Api.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // إضافة new keyword لحل مشكلة الإخفاء
        public new bool EmailConfirmed { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        // العلاقات
        public Hr? HrProfile { get; set; }
        public ICollection<CV> CVs { get; set; } = new List<CV>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
        public ICollection<SimulationResult> SimulationResults { get; set; } = new List<SimulationResult>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<UserGoal> UserGoals { get; set; } = new List<UserGoal>();
    }
}