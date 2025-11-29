// Models/CvAttachment.cs
using CVision.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CvAttachment
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("CV")]
    public int CvId { get; set; }
    public CV? CV { get; set; } // جعلها nullable

    public string FileName { get; set; } = string.Empty; // قيمة افتراضية
    public string FileUrl { get; set; } = string.Empty; // قيمة افتراضية
    public string ContentType { get; set; } = string.Empty; // قيمة افتراضية
    public DateTime UploadedAt { get; set; } = DateTime.Now;
}