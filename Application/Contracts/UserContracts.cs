using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.Contracts;

/// <summary>
/// Payload required to create a new user account.
/// </summary>
public sealed class CreateUserRequest
{
    /// <summary>
    /// Full name of the user.
    /// </summary>
    /// <example>Jane Doe</example>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Valid email address for the user account. Must be unique.
    /// </summary>
    /// <example>jane.doe@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Initial plain-text password for the user. Will be securely hashed upon creation.
    /// </summary>
    /// <example>SecurePass@123</example>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Role identifier to assign to the user (e.g. 1 for Member, 2 for Super Admin).
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Role selection is required.")]
    public int? RoleId { get; set; }

    /// <summary>
    /// Designation identifier to assign to the user.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Designation selection is required.")]
    public int? DesignationId { get; set; }

    /// <summary>
    /// Contact telephone number.
    /// </summary>
    /// <example>+1 (555) 234-5678</example>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Age in years.
    /// </summary>
    /// <example>28</example>
    public int Age { get; set; }

    /// <summary>
    /// Residential or physical street address.
    /// </summary>
    /// <example>742 Evergreen Terrace, Springfield</example>
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Payload to update an existing user's profile and credentials.
/// </summary>
public sealed class UpdateUserRequest
{
    /// <summary>
    /// Updated full name of the user.
    /// </summary>
    /// <example>Jane Doe</example>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Updated email address.
    /// </summary>
    /// <example>jane.doe@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional updated password. If empty or omitted, existing password remains unchanged.
    /// </summary>
    /// <example>NewSecurePass@123</example>
    public string? Password { get; set; }

    /// <summary>
    /// Updated Role identifier assignment.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Role selection is required.")]
    public int? RoleId { get; set; }

    /// <summary>
    /// Updated Designation identifier assignment.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Designation selection is required.")]
    public int? DesignationId { get; set; }

    /// <summary>
    /// Updated contact telephone number.
    /// </summary>
    /// <example>+1 (555) 234-5678</example>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Updated age in years.
    /// </summary>
    /// <example>29</example>
    public int Age { get; set; }

    /// <summary>
    /// Updated street address.
    /// </summary>
    /// <example>742 Evergreen Terrace, Springfield</example>
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// User representation returned in API responses.
/// </summary>
public sealed class UserDto
{
    /// <summary>
    /// Unique user ID.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Full display name.
    /// </summary>
    /// <example>Jane Doe</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Account email address.
    /// </summary>
    /// <example>jane.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User profile image path or URL.
    /// </summary>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// Associated role ID.
    /// </summary>
    /// <example>1</example>
    public int? RoleId { get; set; }

    /// <summary>
    /// Associated role name.
    /// </summary>
    /// <example>Admin</example>
    public string? RoleName { get; set; }

    /// <summary>
    /// Associated designation ID.
    /// </summary>
    /// <example>1</example>
    public int? DesignationId { get; set; }

    /// <summary>
    /// Associated designation name.
    /// </summary>
    /// <example>Software Engineer</example>
    public string? DesignationName { get; set; }

    /// <summary>
    /// Contact telephone number.
    /// </summary>
    /// <example>+1 (555) 234-5678</example>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// User age.
    /// </summary>
    /// <example>28</example>
    public int Age { get; set; }

    /// <summary>
    /// Residential or physical street address.
    /// </summary>
    /// <example>742 Evergreen Terrace, Springfield</example>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Status flag (1 = Active, 0 = Deactivated/Soft-deleted).
    /// </summary>
    /// <example>1</example>
    public int DeletedFlag { get; set; } = 1;

    /// <summary>
    /// Flag indicating whether the user must change their password on initial login.
    /// </summary>
    /// <example>false</example>
    public bool IsFirstLogin { get; set; }
}
