using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents a category business object for workspace compliance and security reports.
    /// </summary>
    [Table("report_categories")]
    public class ReportCategory
    {
        /// <summary>
        /// Unique report category identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the report category (e.g. Compliance, Security, Role Mapping).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of this category.
        /// </summary>
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Active status flag (1 = Active, 0 = Deactivated / Soft-deleted).
        /// </summary>
        public int DeletedFlag { get; set; } = 1;

        /// <summary>
        /// Timestamp when the category was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Report Category.
        /// </summary>
        public static ReportCategory Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Report category name is required.", nameof(name));

            return new ReportCategory
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DeletedFlag = 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Updates the category details.
        /// </summary>
        public void UpdateDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Report category name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Soft deletes the report category.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
        }

        /// <summary>
        /// Restores a soft-deleted report category.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
        }

        #endregion
    }
}
