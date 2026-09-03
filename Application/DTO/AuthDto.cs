using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.DTO;

/// <summary>
/// User login credentials payload.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// Registered email address of the user.
    /// </summary>
    /// <example>admin@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for account authentication.
    /// </summary>
    /// <example>Password123!</example>
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request payload to initiate a password reset OTP dispatch.
/// </summary>
public sealed class ForgotPasswordRequest
{
    /// <summary>
    /// Registered email address to receive the verification OTP.
    /// </summary>
    /// <example>user@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request payload to verify a dispatched OTP code.
/// </summary>
public sealed class VerifyOtpRequest
{
    /// <summary>
    /// Registered email address associated with the OTP.
    /// </summary>
    /// <example>user@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 6-digit numeric OTP code received via email.
    /// </summary>
    /// <example>682491</example>
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
    public string Otp { get; set; } = string.Empty;
}

/// <summary>
/// Request payload to complete password reset using a verified OTP.
/// </summary>
public sealed class ResetPasswordRequest
{
    /// <summary>
    /// Registered email address of the account.
    /// </summary>
    /// <example>user@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 6-digit verification OTP code.
    /// </summary>
    /// <example>682491</example>
    [Required]
    public string Otp { get; set; } = string.Empty;

    /// <summary>
    /// Strong new password (requires uppercase, lowercase, digit, special character, min 8 chars).
    /// </summary>
    /// <example>Secure@P4ssw0rd!</example>
    [Required]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of the new password.
    /// </summary>
    /// <example>Secure@P4ssw0rd!</example>
    public string? ConfirmPassword { get; set; }
}

/// <summary>
/// Authentication payload containing authenticated user identity, role, permissions, dynamic menus, and JWT bearer token.
/// </summary>
public sealed class AuthUserData
{
    /// <summary>
    /// Unique user identifier.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Full display name of the user.
    /// </summary>
    /// <example>Alex Morgan</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the user.
    /// </summary>
    /// <example>admin@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// URL or relative path to the user's uploaded profile picture.
    /// </summary>
    /// <example>/uploads/profiles/user_1_abc123.jpg</example>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// Assigned Role identifier (e.g. 2 for Super Admin).
    /// </summary>
    /// <example>2</example>
    public int? RoleId { get; set; }

    /// <summary>
    /// Human-readable name of the assigned role.
    /// </summary>
    /// <example>Super Admin</example>
    public string? RoleName { get; set; }

    /// <summary>
    /// Associated organizational department name.
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Associated job designation title.
    /// </summary>
    public string? DesignationName { get; set; }

    /// <summary>
    /// List of permission keys granted to this user.
    /// </summary>
    /// <example>["users.view", "users.create", "roles.view", "permissions.manage"]</example>
    public List<string> Permissions { get; set; } = [];

    /// <summary>
    /// List of dynamic navigation menus accessible by the user.
    /// </summary>
    public List<MenuItemDto> Menus { get; set; } = [];

    /// <summary>
    /// Encrypted JWT bearer token to be included in subsequent requests via the Authorization header.
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the user must change password on their initial login.
    /// </summary>
    /// <example>false</example>
    public bool IsFirstLogin { get; set; }
}

/// <summary>
/// Response returned by the current user permissions endpoint.
/// </summary>
public sealed class CurrentUserPermissionsResponse
{
    /// <summary>
    /// Active permission keys assigned to the current user's role.
    /// </summary>
    /// <example>["dashboard.view", "users.view", "roles.view"]</example>
    public List<string> Permissions { get; set; } = [];
}

/// <summary>
/// Request payload to verify 2FA OTP code and complete login.
/// </summary>
public sealed class Verify2FaLoginRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    /// <example>admin@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 6-digit 2FA OTP code dispatched via email.
    /// </summary>
    /// <example>682491</example>
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
    public string Otp { get; set; } = string.Empty;
}

/// <summary>
/// Request payload to re-send 2FA OTP during login.
/// </summary>
public sealed class Resend2FaOtpRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    /// <example>admin@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Login result payload supporting both direct JWT issuance and 2FA challenge.
/// </summary>
public sealed class LoginResponse
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Status or instructional message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether Two-Factor Authentication is active and requires OTP input.
    /// </summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>
    /// Authenticated user session data (present when 2FA is not required or after successful OTP validation).
    /// </summary>
    public AuthUserData? Data { get; set; }
}

/// <summary>
/// Request payload to evaluate password complexity and strength.
/// </summary>
public sealed class EvaluatePasswordRequest
{
    /// <summary>
    /// Password string to evaluate.
    /// </summary>
    /// <example>Admin@12345</example>
    public string? Password { get; set; }
}

/// <summary>
/// Status of individual password policy criteria.
/// </summary>
public sealed class PasswordEvaluationCriteriaDto
{
    /// <summary>
    /// Whether the password has at least 8 characters.
    /// </summary>
    public bool MinLength { get; set; }

    /// <summary>
    /// Whether the password contains at least one uppercase letter.
    /// </summary>
    public bool HasUpper { get; set; }

    /// <summary>
    /// Whether the password contains at least one lowercase letter.
    /// </summary>
    public bool HasLower { get; set; }

    /// <summary>
    /// Whether the password contains at least one numeric digit.
    /// </summary>
    public bool HasNumber { get; set; }

    /// <summary>
    /// Whether the password contains at least one special character.
    /// </summary>
    public bool HasSpecial { get; set; }
}

/// <summary>
/// Detailed result of backend password evaluation.
/// </summary>
public sealed class EvaluatePasswordResponse
{
    /// <summary>
    /// Whether the password satisfies all strong security requirements.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Alias for IsValid indicating if the password meets strong criteria.
    /// </summary>
    public bool IsStrong { get; set; }

    /// <summary>
    /// Password strength score from 0 to 100.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Descriptive strength label ("Empty", "Very Weak", "Weak", "Fair", "Good", "Strong").
    /// </summary>
    public string StrengthLabel { get; set; } = string.Empty;

    /// <summary>
    /// Breakdown of individual security criteria statuses.
    /// </summary>
    public PasswordEvaluationCriteriaDto Criteria { get; set; } = new();

    /// <summary>
    /// List of unmet policy error/requirement messages.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Summary feedback message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Payload to record user logout session.
/// </summary>
public sealed class LogoutRequest
{
    /// <summary>
    /// Optional user ID.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Optional user email address.
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// DTO representing a persisted user login session record.
/// </summary>
public sealed class UserSessionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = "127.0.0.1";
    public string? UserAgent { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string? SessionToken { get; set; }
    public bool IsActive { get; set; }
    public int DeletedFlag { get; set; } = 1;
}

/// <summary>
/// Google Sign-In authentication request payload.
/// </summary>
public sealed class GoogleLoginRequest
{
    /// <summary>
    /// Google OAuth ID Token (JWT) returned by Google Identity Services.
    /// </summary>
    public string? IdToken { get; set; }

    /// <summary>
    /// Email extracted from Google OAuth.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Full Name provided by Google.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Google avatar picture URL.
    /// </summary>
    public string? ProfileImage { get; set; }
}

/// <summary>
/// System maintenance mode status response.
/// </summary>
public sealed class MaintenanceStatusResponse
{
    public bool IsMaintenanceMode { get; set; }
    public string Message { get; set; } = string.Empty;
}
