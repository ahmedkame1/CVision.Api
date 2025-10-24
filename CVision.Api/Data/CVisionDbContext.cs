using CVision.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CVision.Api.Data
{
    public class CVisionDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public CVisionDbContext(DbContextOptions<CVisionDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hr> Hrs { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<CvAttachment> CvAttachments { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<SimulationResult> SimulationResults { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<UserGoal> UserGoals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // علاقات Identity موروثة تلقائياً

            // HR Relationships
            modelBuilder.Entity<Hr>()
                .HasIndex(h => h.UserId)
                .IsUnique();

            modelBuilder.Entity<Hr>()
                .HasOne(h => h.User)
                .WithOne(u => u.HrProfile)
                .HasForeignKey<Hr>(h => h.UserId);

            modelBuilder.Entity<Hr>()
                .HasMany(h => h.Jobs)
                .WithOne(j => j.Hr)
                .HasForeignKey(j => j.HrId);

            // User Relationships
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.CVs)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Applications)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.TestResults)
                .WithOne(tr => tr.User)
                .HasForeignKey(tr => tr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.SimulationResults)
                .WithOne(sr => sr.User)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.UserGoals)
                .WithOne(ug => ug.User)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Job Relationships
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Applications)
                .WithOne(a => a.Job)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // CV Relationships
            modelBuilder.Entity<CV>()
                .HasMany(c => c.Attachments)
                .WithOne(ca => ca.CV)
                .HasForeignKey(ca => ca.CvId)
                .OnDelete(DeleteBehavior.Cascade);

            // Application Relationships
            modelBuilder.Entity<Application>()
                .HasOne(a => a.CV)
                .WithMany()
                .HasForeignKey(a => a.CvId)
                .OnDelete(DeleteBehavior.Restrict);

            // Test Relationships
            modelBuilder.Entity<Test>()
                .HasMany(t => t.TestResults)
                .WithOne(tr => tr.Test)
                .HasForeignKey(tr => tr.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Goal Relationships
            modelBuilder.Entity<Goal>()
                .HasMany(g => g.UserGoals)
                .WithOne(ug => ug.Goal)
                .HasForeignKey(ug => ug.GoalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}