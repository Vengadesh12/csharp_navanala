using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Models
{
    [Table("departmentpermissions")]
    public class DepartmentPermissionModel
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public int PermissionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
