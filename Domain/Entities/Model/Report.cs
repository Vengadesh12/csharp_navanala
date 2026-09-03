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

        #region Business Object Domain Methods

        public static Report Create(
            string title,
            string? description,
            int? categoryId,
            string? category,
            string? format,
            string? createdBy,
            string? status = "Ready",
            string? fileSize = "1.5 MB",
            string? fileName = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Report title is required.", nameof(title));

            var now = DateTime.UtcNow;
            return new Report
            {
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                CategoryId = categoryId,
                Category = string.IsNullOrWhiteSpace(category) ? "Compliance" : category.Trim(),
                Format = string.IsNullOrWhiteSpace(format) ? "PDF" : format.Trim(),
                CreatedBy = createdBy?.Trim() ?? string.Empty,
                Status = string.IsNullOrWhiteSpace(status) ? "Ready" : status.Trim(),
                FileSize = string.IsNullOrWhiteSpace(fileSize) ? "1.5 MB" : fileSize.Trim(),
                FileName = fileName,
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        public void UpdateDetails(
            string title,
            string? description,
            int? categoryId,
            string? category,
            string? format,
            string? status,
            string? fileSize = null,
            string? fileName = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Report title cannot be empty.", nameof(title));

            Title = title.Trim();
            Description = description?.Trim() ?? string.Empty;
            CategoryId = categoryId;
            if (!string.IsNullOrWhiteSpace(category)) Category = category.Trim();
            if (!string.IsNullOrWhiteSpace(format)) Format = format.Trim();
            if (!string.IsNullOrWhiteSpace(status)) Status = status.Trim();
            if (!string.IsNullOrWhiteSpace(fileSize)) FileSize = fileSize.Trim();
            if (!string.IsNullOrWhiteSpace(fileName)) FileName = fileName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateFile(string fileName, string fileSize, string? format = null)
        {
            if (!string.IsNullOrWhiteSpace(fileName)) FileName = fileName.Trim();
            if (!string.IsNullOrWhiteSpace(fileSize)) FileSize = fileSize.Trim();
            if (!string.IsNullOrWhiteSpace(format)) Format = format.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
