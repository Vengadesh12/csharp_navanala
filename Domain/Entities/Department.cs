using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents an organizational department business object with hierarchical aggregation.
    /// </summary>
    [Table("departments")]
    public class Department
    {
        /// <summary>
        /// Unique department identifier.
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Name of the department.
        /// </summary>
        /// <example>Software Development</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the department functions and scope.
        /// </summary>
        /// <example>Core engineering, application architecture, and development teams.</example>
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Active status flag (1 = Active, 0 = Deactivated).
        /// </summary>
        /// <example>1</example>
        public int DeletedFlag { get; set; } = 1;

        /// <summary>
        /// Timestamp when the department was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the department was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collection of job designations mapped under this department.
        /// </summary>
        public ICollection<Designation> Designations { get; set; } = new List<Designation>();

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Department.
        /// </summary>
        public static Department Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Department name is required.", nameof(name));

            var now = DateTime.UtcNow;
            return new Department
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DeletedFlag = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        /// <summary>
        /// Updates the department name and description.
        /// </summary>
        public void UpdateDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Department name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the department.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Restores a soft-deleted department.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
