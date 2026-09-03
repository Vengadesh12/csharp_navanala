using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MyBackend.Application.DTO;

/// <summary>
/// Payload to update the list of permission keys assigned to a specific role.
/// </summary>
public sealed class UpdatePermissionsRequest
{
    /// <summary>
    /// Array of permission keys to assign to the role.
    /// </summary>
    /// <example>["dashboard.view", "users.view", "users.create", "roles.view"]</example>
    [Required]
    [JsonPropertyName("permissionKeys")]
    public string[] PermissionKeys { get; set; } = [];
}

/// <summary>
/// Individual system permission details.
/// </summary>
public sealed class PermissionDto
{
    /// <summary>
    /// Unique permission key identifier (e.g. users.create, roles.view).
    /// </summary>
    /// <example>users.create</example>
    public string PermissionKey { get; set; } = string.Empty;

    /// <summary>
    /// Human-friendly permission name.
    /// </summary>
    /// <example>Create Users</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of capabilities granted by this permission.
    /// </summary>
    /// <example>Allows provisioning new member accounts and assigning roles.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this permission is assigned (1 = assigned, 0 = unassigned).
    /// </summary>
    /// <example>1</example>
    public int IsAssigned { get; set; }
}

/// <summary>
/// Role with its comma-delimited assigned permission keys from the database.
/// </summary>
public sealed class RolePermissionDto
{
    /// <summary>
    /// Role identifier.
    /// </summary>
    /// <example>1</example>
    public int RoleId { get; set; }

    /// <summary>
    /// Name of the role.
    /// </summary>
    /// <example>Admin</example>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of assigned permission keys.
    /// </summary>
    /// <example>users.view,users.create,roles.view</example>
    public string PermissionKeys { get; set; } = string.Empty;
}

/// <summary>
/// Formatted role permission matrix item returned to client applications.
/// </summary>
public sealed class RolePermissionMatrixItem
{
    /// <summary>
    /// Unique role identifier.
    /// </summary>
    /// <example>1</example>
    public int RoleId { get; set; }

    /// <summary>
    /// Display name of the role.
    /// </summary>
    /// <example>Admin</example>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Array of permission keys currently assigned to this role.
    /// </summary>
    /// <example>["users.view", "users.create", "roles.view"]</example>
    public string[] PermissionKeys { get; set; } = [];
}

/// <summary>
/// Department with its comma-delimited assigned permission keys from the database.
/// </summary>
public sealed class DepartmentPermissionDto
{
    /// <summary>
    /// Department identifier.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Name of the department.
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of assigned permission keys.
    /// </summary>
    public string PermissionKeys { get; set; } = string.Empty;
}

/// <summary>
/// Formatted department permission matrix item returned to client applications.
/// </summary>
public sealed class DepartmentPermissionMatrixItem
{
    /// <summary>
    /// Unique department identifier.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Display name of the department.
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Array of permission keys currently assigned to this department.
    /// </summary>
    public string[] PermissionKeys { get; set; } = [];
}

/// <summary>
/// Full permissions matrix response containing all system permissions and assignments across roles and departments.
/// </summary>
public sealed class PermissionsMatrixResponse
{
    /// <summary>
    /// All available permissions registered in the system.
    /// </summary>
    public List<PermissionDto> Permissions { get; set; } = [];

    /// <summary>
    /// Matrix of all roles and their assigned permission keys.
    /// </summary>
    public List<RolePermissionMatrixItem> Roles { get; set; } = [];

    /// <summary>
    /// Matrix of all departments and their assigned permission keys.
    /// </summary>
    public List<DepartmentPermissionMatrixItem> Departments { get; set; } = [];
}
