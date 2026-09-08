using System;

namespace MyBackend.Domain.Models
{
    public class MenuModel
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
    }
}
