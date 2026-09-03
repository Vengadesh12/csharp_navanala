using System;

namespace MyBackend.Domain.Entities
{
    public class EventType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#3b82f6";
        public string Icon { get; set; } = "Event";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "System Admin";
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        public static EventType Create(
            string name,
            string? description,
            string? color,
            string? icon,
            string? createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Event type name is required.", nameof(name));

            var now = DateTime.UtcNow;
            return new EventType
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Color = string.IsNullOrWhiteSpace(color) ? "#3b82f6" : color.Trim(),
                Icon = string.IsNullOrWhiteSpace(icon) ? "Event" : icon.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "System Admin" : createdBy.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        public void UpdateDetails(string? description, string? color, string? icon)
        {
            Description = description?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(color)) Color = color.Trim();
            if (!string.IsNullOrWhiteSpace(icon)) Icon = icon.Trim();
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
