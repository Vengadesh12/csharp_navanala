using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.Contracts;

/// <summary>
/// Representation of a workspace designation returned in API responses.
/// </summary>
public sealed class DesignationDto
{
    /// <summary>
    /// Unique designation ID.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Name / Title of the designation.
    /// </summary>
    /// <example>Software Engineer</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of the designation.
    /// </summary>
    /// <example>Develops and maintains core applications and services.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Status flag (1 = Active, 0 = Deactivated).
    /// </summary>
    /// <example>1</example>
    public int DeletedFlag { get; set; } = 1;

    /// <summary>
    /// Associated Department ID if assigned.
    /// </summary>
    /// <example>1</example>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Associated Department Name.
    /// </summary>
    /// <example>Software Development</example>
    public string? DepartmentName { get; set; }
}

/// <summary>
/// Payload to create a new designation in the system.
/// </summary>
public sealed class CreateDesignationRequest
{
    /// <summary>
    /// Name of the designation.
    /// </summary>
    /// <example>Software Engineer</example>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the designation.
    /// </summary>
    /// <example>Develops and maintains core applications and services.</example>
    [MaxLength(255)]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional Department ID to assign this designation to.
    /// </summary>
    public int? DepartmentId { get; set; }
}

/// <summary>
/// Payload to update an existing designation in the system.
/// </summary>
public sealed class UpdateDesignationRequest
{
    /// <summary>
    /// Name of the designation.
    /// </summary>
    /// <example>Software Engineer</example>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the designation.
    /// </summary>
    /// <example>Develops and maintains core applications and services.</example>
    [MaxLength(255)]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional Department ID to assign this designation to.
    /// </summary>
    public int? DepartmentId { get; set; }
}
