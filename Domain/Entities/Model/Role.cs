using System;

namespace MyBackend.Domain.Entities.Model
{
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DeletedFlag { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        #region Business Object Domain Methods

        public static Role Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required.", nameof(name));

            var now = DateTime.UtcNow;
            return new Role
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DeletedFlag = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public void UpdateDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
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
