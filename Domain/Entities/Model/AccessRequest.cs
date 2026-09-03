using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities.Model
{
    [Table("access_requests")]
    public class AccessRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string? DepartmentName { get; set; }

        public string? RoleName { get; set; }

        public string PermissionKey { get; set; } = string.Empty;

        public string PermissionName { get; set; } = string.Empty;

        public string? Module { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium";

        public string Status { get; set; } = "Pending";

        public int? ReviewerId { get; set; }

        public string? ReviewerName { get; set; }

        public string? ReviewerComments { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int DeletedFlag { get; set; } = 1;
    }
}
