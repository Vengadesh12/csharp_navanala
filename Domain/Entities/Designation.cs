using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    [Table("designations")]
    public class Designation
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        public int DeletedFlag { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public Department? Department { get; set; }

        #region Business Object Domain Methods

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

        public void UpdateDetails(string name, string? description, int? departmentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Designation name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            DepartmentId = departmentId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignDepartment(int departmentId)
        {
            DepartmentId = departmentId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UnassignDepartment()
        {
            DepartmentId = null;
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
