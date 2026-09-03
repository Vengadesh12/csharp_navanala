using System;

namespace MyBackend.Domain.Entities.Model
{
    public class Report
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string Category { get; set; } = "Compliance";
        public string Format { get; set; } = "PDF";
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Generated";
        public string FileSize { get; set; } = "1.2 MB";
        public string? FileName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int DeletedFlag { get; set; } = 1;
    }
}
