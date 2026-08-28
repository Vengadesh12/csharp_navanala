using System;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents a dynamic navigation menu item business object in the application.
    /// </summary>
    public class Menu
    {
        /// <summary>
        /// Unique menu identifier.
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Unique menu key string identifier.
        /// </summary>
        /// <example>dashboard.view</example>
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// Navigation menu display label.
        /// </summary>
        /// <example>Dashboard</example>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Icon symbol or icon identifier.
        /// </summary>
        /// <example>◫</example>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Client-side application route path.
        /// </summary>
        /// <example>/dashboard</example>
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// Navigation grouping section name.
        /// </summary>
        /// <example>Core Access</example>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Description of the menu section and functionality.
        /// </summary>
        /// <example>System metrics &amp; access summary</example>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Sorting and display order index.
        /// </summary>
        /// <example>1</example>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Required permission key to view/access this menu (null if public to all authenticated users).
        /// </summary>
        /// <example>dashboard.view</example>
        public string? PermissionKey { get; set; }

        /// <summary>
        /// Status flag (1 = Active, 0 = Deleted).
        /// </summary>
        /// <example>1</example>
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Navigation Menu entry.
        /// </summary>
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
                DeletedFlag = 1
            };
        }

        /// <summary>
        /// Updates the navigation menu details.
        /// </summary>
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
        }

        /// <summary>
        /// Soft deletes the menu item.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
        }

        /// <summary>
        /// Restores a soft-deleted menu item.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
        }

        #endregion
    }
}
