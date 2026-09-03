using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.DTO;

public sealed class CreateRoleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public sealed class UpdateRoleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public sealed class RoleDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DeletedFlag { get; set; } = 1;
}
