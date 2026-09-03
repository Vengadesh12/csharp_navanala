using System;

namespace MyBackend.Domain.Entities.Model
{
    public class Menu
    {
        public int Id { get; set; }

        public string MenuKey { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string Route { get; set; } = string.Empty;

        public string GroupName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        public string? PermissionKey { get; set; }

        public int DeletedFlag { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        #region Business Object Domain Methods

        public static Menu Create(
            string menuKey,
            string label,
            string icon,
            string route,
            string groupName,
            string? description,
            int orderIndex,
            string? permissionKey = null)
        {
            if (string.IsNullOrWhiteSpace(menuKey))
                throw new ArgumentException("Menu key is required.", nameof(menuKey));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Menu label is required.", nameof(label));

            var now = DateTime.UtcNow;
            return new Menu
            {
                MenuKey = menuKey.Trim(),
                Label = label.Trim(),
                Icon = icon?.Trim() ?? string.Empty,
                Route = route?.Trim() ?? string.Empty,
                GroupName = groupName?.Trim() ?? "Core Access",
                Description = description?.Trim() ?? string.Empty,
                OrderIndex = orderIndex,
                PermissionKey = string.IsNullOrWhiteSpace(permissionKey) ? null : permissionKey.Trim(),
                DeletedFlag = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public void UpdateDetails(
            string label,
            string icon,
            string route,
            string groupName,
            string? description,
            int orderIndex,
            string? permissionKey)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Menu label cannot be empty.", nameof(label));

            Label = label.Trim();
            Icon = icon?.Trim() ?? string.Empty;
            Route = route?.Trim() ?? string.Empty;
            GroupName = groupName?.Trim() ?? "Core Access";
            Description = description?.Trim() ?? string.Empty;
            OrderIndex = orderIndex;
            PermissionKey = string.IsNullOrWhiteSpace(permissionKey) ? null : permissionKey.Trim();
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
