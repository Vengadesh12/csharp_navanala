using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Models
{
    [Table("userpermissions")]
    public class UserPermissionModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
