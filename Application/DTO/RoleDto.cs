using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.DTO;

/// <summary>
/// Payload to create a new workspace role.
/// </summary>
public sealed class CreateRoleRequest
{
    /// <summary>
    /// Unique name of the role (e.g. Editor, Compliance Manager).
    /// </summary>
    /// <example>Auditor</example>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description detailing the purpose and access level of the role.
    /// </summary>
    /// <example>Grants read-only access to audit trails and activity logs.</example>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Payload to update an existing role's title and description.
/// </summary>
public sealed class UpdateRoleRequest
{
    /// <summary>
    /// Updated name of the role.
    /// </summary>
    /// <example>Senior Auditor</example>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Updated description of the role.
    /// </summary>
    /// <example>Grants comprehensive access to workspace audit trails and compliance reports.</example>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Role representation data returned in API responses.
/// </summary>
public sealed class RoleDto
{
    /// <summary>
    /// Unique role identifier.
    /// </summary>
    /// <example>3</example>
    public int Id { get; set; }

    /// <summary>
    /// Role name.
    /// </summary>
    /// <example>Auditor</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role description.
    /// </summary>
    /// <example>Grants read-only access to audit logs.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Status flag (1 = Active, 0 = Inactive/Deleted).
    /// </summary>
    /// <example>1</example>
    public int DeletedFlag { get; set; } = 1;
}
