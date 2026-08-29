using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents a category business object for workspace project initiatives.
    /// </summary>
    [Table("project_categories")]
    public class ProjectCategory
    {
        /// <summary>
        /// Unique project category identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the project category (e.g. RBAC Rollout, DevOps, Security).
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

        /// <summary>
        /// Timestamp when the category was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Project Category.
        /// </summary>
        public static ProjectCategory Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project category name is required.", nameof(name));

            var now = DateTime.UtcNow;
            return new ProjectCategory
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DeletedFlag = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        /// <summary>
        /// Updates the category details.
        /// </summary>
        public void UpdateDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project category name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the category.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Restores a soft deleted category.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
