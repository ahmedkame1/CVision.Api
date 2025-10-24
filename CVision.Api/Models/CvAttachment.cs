using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVision.Api.Models
{
    public class CvAttachment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CV")]
        public int CvId { get; set; }
        public CV CV { get; set; }

        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
