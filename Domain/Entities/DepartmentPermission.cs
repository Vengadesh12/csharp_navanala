using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents the association between an organizational Department and a granted Permission.
    /// Users belonging to designations mapped under this department inherit these permissions.
    /// </summary>
    [Table("departmentpermissions")]
    public class DepartmentPermission
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public int PermissionId { get; set; }
        public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;
        public System.DateTime? UpdatedAt { get; set; } = System.DateTime.UtcNow;

        public static DepartmentPermission Create(int departmentId, int permissionId) => new()
        {
            DepartmentId = departmentId,
            PermissionId = permissionId,
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
        };
    }
}
