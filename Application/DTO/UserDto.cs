using System;
using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.DTO;

public sealed class CreateUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role selection is required.")]
    public int? RoleId { get; set; }

    [Required(ErrorMessage = "Designation selection is required.")]
    public int? DesignationId { get; set; }

    public string Phone { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Address { get; set; } = string.Empty;
}

public sealed class UpdateUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Required(ErrorMessage = "Role selection is required.")]
    public int? RoleId { get; set; }

    [Required(ErrorMessage = "Designation selection is required.")]
    public int? DesignationId { get; set; }

    public string Phone { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Address { get; set; } = string.Empty;
}

public sealed class UserDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? ProfileImage { get; set; }

    public int? RoleId { get; set; }

    public string? RoleName { get; set; }

    public int? DesignationId { get; set; }

    public string? DesignationName { get; set; }

    public string Phone { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Address { get; set; } = string.Empty;

    public int DeletedFlag { get; set; } = 1;

    public bool IsFirstLogin { get; set; }
}
