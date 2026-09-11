using System;

namespace MyBackend.Domain.Models
{
    // ==============================================================================
    // TOPIC: Role-Based Access Control (RBAC)
    // TOPIC: Implement Hierarchical Role-Based Access Control with Permission Inheritance
    // Represents a system or organizational role.
    // ParentRoleId establishes the parent-child relationship for permission inheritance.
    // ==============================================================================
    public class RoleModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // TOPIC: Hierarchical RBAC - Parent role link for permission inheritance
        public int? ParentRoleId { get; set; }

        public RoleModel? ParentRole { get; set; }

        public int DeletedFlag { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
