using System;
using System.Collections.Generic;

namespace MyBackend.Application.Common.DTO
{
    public class UserPermissionOverviewDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public int DirectPermissionsCount { get; set; }
        public int RolePermissionsCount { get; set; }
        public int DepartmentPermissionsCount { get; set; }
        public int TotalEffectivePermissionsCount { get; set; }
    }

    public class UserPermissionDetailDto
    {
        public int PermissionId { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Access { get; set; } = "Allow";
        public bool IsAllowed { get; set; }
        public bool IsDirect { get; set; }
        public bool IsFromRole { get; set; }
        public bool IsFromDepartment { get; set; }
        public string Source { get; set; } = string.Empty; // "UserDirectGrant", "SuperAdmin", "Role", "Department", "RoleAndDepartment", "DefaultDeny"
        public string? DepartmentName { get; set; }
        public string? RoleName { get; set; }
        public int? UserPermissionId { get; set; }
        public DateTime? GrantedAt { get; set; }
    }

    public class UserPermissionProfileDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public int DirectCount { get; set; }
        public int RoleCount { get; set; }
        public int DepartmentCount { get; set; }
        public int TotalCount { get; set; }
        public List<UserPermissionDetailDto> Permissions { get; set; } = new();
    }

    public class AssignUserPermissionRequest
    {
        public string PermissionKey { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
