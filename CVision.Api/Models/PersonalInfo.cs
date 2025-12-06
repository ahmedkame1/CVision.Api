// Models/PersonalInfo.cs
using CVision.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PersonalInfo
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("CV")]
    public int CvId { get; set; }
    public CV CV { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LinkedIn { get; set; }

    [MaxLength(200)]
    public string? GitHub { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }
}