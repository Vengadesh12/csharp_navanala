using System;

namespace MyBackend.Domain.Models
{
    // ==============================================================================
    // TOPIC: Role-Based Access Control (RBAC)
    // TOPIC: Permission conflict resolution
    // Associates a Permission with a Role.
    // 'Access' can be 'Allow' or 'Deny', driving deterministic conflict resolution.
    // ==============================================================================
    public class RolePermissionModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        // TOPIC: Permission conflict resolution - Explicit Allow or Deny rule
        public string Access { get; set; } = "Allow";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
