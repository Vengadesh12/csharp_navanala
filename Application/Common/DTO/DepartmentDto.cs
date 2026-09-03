using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.Common.DTO;

public sealed class DepartmentDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DeletedFlag { get; set; } = 1;

    public DateTime CreatedAt { get; set; }

    public int DesignationCount { get; set; }

    public int UserCount { get; set; }

    public List<DesignationDto> Designations { get; set; } = new();
}

public sealed class CreateDepartmentRequest
{
    [Required(ErrorMessage = "Department name is required.")]
    [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
    public string? Description { get; set; } = string.Empty;

    public List<int>? DesignationIds { get; set; }
}

public sealed class UpdateDepartmentRequest
{
    [Required(ErrorMessage = "Department name is required.")]
    [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
    public string? Description { get; set; } = string.Empty;

    public List<int>? DesignationIds { get; set; }
}

public sealed class MapDepartmentDesignationsRequest
{
    [Required(ErrorMessage = "At least one designation ID is required.")]
    public List<int> DesignationIds { get; set; } = new();
}

public sealed class DepartmentOverviewResponse
{
    public int TotalDepartments { get; set; }

    public int TotalDesignations { get; set; }

    public int MappedDesignations { get; set; }

    public int UnassignedDesignations { get; set; }

    public List<DepartmentDto> Departments { get; set; } = new();

    public List<DesignationDto> UnassignedList { get; set; } = new();
}
