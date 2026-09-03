using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.DTO;

public sealed class DesignationDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DeletedFlag { get; set; } = 1;

    public int? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }
}

public sealed class CreateDesignationRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }
}

public sealed class UpdateDesignationRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }
}
