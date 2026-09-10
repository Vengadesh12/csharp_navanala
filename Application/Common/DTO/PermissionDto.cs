using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MyBackend.Application.Common.DTO;

public sealed class UpdateRolePermissionRule
{
    public string PermissionKey { get; set; } = string.Empty;

    public string Access { get; set; } = "Allow"; // "Allow" or "Deny"
}

public sealed class UpdatePermissionsRequest
{
    [JsonPropertyName("permissionKeys")]
    public string[] PermissionKeys { get; set; } = [];

    [JsonPropertyName("rules")]
    public List<UpdateRolePermissionRule>? Rules { get; set; }
}

public sealed class EffectivePermissionDto
{
    public string PermissionKey { get; set; } = string.Empty;

    public string Menu { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string Access { get; set; } = "Allow"; // "Allow" or "Deny"

    public bool IsAllowed { get; set; }

    public string Source { get; set; } = "Direct"; // "Direct", "Inherited", "SuperAdmin", "DefaultDeny"

    public string? InheritedFromRole { get; set; }
}

public sealed class RoleHierarchyDto
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int? ParentRoleId { get; set; }

    public string? ParentRoleName { get; set; }

    public List<RoleHierarchyDto> Children { get; set; } = [];

    public List<EffectivePermissionDto> EffectivePermissions { get; set; } = [];
}

public sealed class PermissionDto
{
    public string PermissionKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int IsAssigned { get; set; }
}

public sealed class RolePermissionDto
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string PermissionKeys { get; set; } = string.Empty;
}

public sealed class RolePermissionMatrixItem
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string[] PermissionKeys { get; set; } = [];
}

public sealed class DepartmentPermissionDto
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string PermissionKeys { get; set; } = string.Empty;
}

public sealed class DepartmentPermissionMatrixItem
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string[] PermissionKeys { get; set; } = [];
}

public sealed class PermissionsMatrixResponse
{
    public List<PermissionDto> Permissions { get; set; } = [];

    public List<RolePermissionMatrixItem> Roles { get; set; } = [];

    public List<DepartmentPermissionMatrixItem> Departments { get; set; } = [];
}
