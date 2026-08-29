using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents an organizational job title / designation business object.
    /// </summary>
    [Table("designations")]
    public class Designation
    {
        /// <summary>
        /// Unique designation identifier.
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Name / Title of the designation.
        /// </summary>
        /// <example>Software Engineer</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Short description of responsibilities for this designation.
        /// </summary>
        /// <example>Develops and maintains core applications and services.</example>
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Optional identifier of the department this designation is assigned to.
        /// </summary>
        /// <example>1</example>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Active status flag (1 = Active, 0 = Deactivated).
        /// </summary>
        /// <example>1</example>
        public int DeletedFlag { get; set; } = 1;

        /// <summary>
        /// Timestamp when the designation record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the designation record was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property to parent department.
        /// </summary>
        public Department? Department { get; set; }

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Designation.
        /// </summary>
        public static Designation Create(string name, string? description, int? departmentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Designation name is required.", nameof(name));

            var now = DateTime.UtcNow;
            return new Designation
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DepartmentId = departmentId,
                DeletedFlag = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        /// <summary>
        /// Updates the designation name, description, and assigned department.
        /// </summary>
        public void UpdateDetails(string name, string? description, int? departmentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Designation name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            DepartmentId = departmentId;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Assigns the designation to a department.
        /// </summary>
        public void AssignDepartment(int departmentId)
        {
            DepartmentId = departmentId;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Unassigns the designation from its current department.
        /// </summary>
        public void UnassignDepartment()
        {
            DepartmentId = null;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the designation.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Restores a soft-deleted designation.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
