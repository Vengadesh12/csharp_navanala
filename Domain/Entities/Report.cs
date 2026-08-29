using System;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Compliance, access, and security reports business object.
    /// </summary>
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Report.
        /// </summary>
        public static Report Create(
            string title,
            string? description,
            int? categoryId,
            string? category,
            string? format,
            string? createdBy,
            string? status = "Ready",
            string? fileSize = "1.5 MB")
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
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        /// <summary>
        /// Updates the report parameters.
        /// </summary>
        public void UpdateDetails(
            string title,
            string? description,
            int? categoryId,
            string? category,
            string? format,
            string? status)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Report title cannot be empty.", nameof(title));

            Title = title.Trim();
            Description = description?.Trim() ?? string.Empty;
            CategoryId = categoryId;
            if (!string.IsNullOrWhiteSpace(category)) Category = category.Trim();
            if (!string.IsNullOrWhiteSpace(format)) Format = format.Trim();
            if (!string.IsNullOrWhiteSpace(status)) Status = status.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the report.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Restores a soft-deleted report.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
