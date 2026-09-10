using System;
using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.Common.DTO;

public sealed class CreateUserRequest : IValidatableObject
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role selection is required.")]
    public int? RoleId { get; set; }

    [Required(ErrorMessage = "Designation selection is required.")]
    public int? DesignationId { get; set; }

    public string Phone { get; set; } = string.Empty;

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
    public int Age { get; set; }

    public string Address { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(Email) &&
            string.Equals(Password.Trim(), Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Password cannot be the same as email.",
                new[] { nameof(Password) }
            );
        }
    }
}

public sealed class UpdateUserRequest : IValidatableObject
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Required(ErrorMessage = "Role selection is required.")]
    public int? RoleId { get; set; }

    [Required(ErrorMessage = "Designation selection is required.")]
    public int? DesignationId { get; set; }

    public string Phone { get; set; } = string.Empty;

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
    public int Age { get; set; }

    public string Address { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(Email) &&
            string.Equals(Password.Trim(), Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Password cannot be the same as email.",
                new[] { nameof(Password) }
            );
        }
    }
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

    public DateTime CreatedAt { get; set; }
}
