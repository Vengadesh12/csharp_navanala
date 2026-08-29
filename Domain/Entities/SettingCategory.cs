using System;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents a configuration category business object under Settings.
    /// </summary>
    public class SettingCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "Tune";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "System Admin";
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Setting Category.
        /// </summary>
        public static SettingCategory Create(
            string name,
            string? description,
            string? icon,
            string? createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Setting category name is required.", nameof(name));

            var now = DateTime.UtcNow;
            return new SettingCategory
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Icon = string.IsNullOrWhiteSpace(icon) ? "Tune" : icon.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "System Admin" : createdBy.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        /// <summary>
        /// Updates the category details.
        /// </summary>
        public void UpdateDetails(string name, string? description, string? icon)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Setting category name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(icon)) Icon = icon.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the setting category.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Restores the setting category.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
