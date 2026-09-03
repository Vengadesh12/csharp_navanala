using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.DTO;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class VerifyOtpRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
    public string Otp { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Otp { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;

    public string? ConfirmPassword { get; set; }
}

public sealed class AuthUserData
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? ProfileImage { get; set; }

    public int? RoleId { get; set; }

    public string? RoleName { get; set; }

    public string? DepartmentName { get; set; }

    public string? DesignationName { get; set; }

    public List<string> Permissions { get; set; } = [];

    public List<MenuItemDto> Menus { get; set; } = [];

    public string Token { get; set; } = string.Empty;

    public bool IsFirstLogin { get; set; }
}

public sealed class CurrentUserPermissionsResponse
{
    public List<string> Permissions { get; set; } = [];
}

public sealed class Verify2FaLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
    public string Otp { get; set; } = string.Empty;
}

public sealed class Resend2FaOtpRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool RequiresTwoFactor { get; set; }

    public AuthUserData? Data { get; set; }
}

public sealed class EvaluatePasswordRequest
{
    public string? Password { get; set; }
}

public sealed class PasswordEvaluationCriteriaDto
{
    public bool MinLength { get; set; }

    public bool HasUpper { get; set; }

    public bool HasLower { get; set; }

    public bool HasNumber { get; set; }

    public bool HasSpecial { get; set; }
}

public sealed class EvaluatePasswordResponse
{
    public bool IsValid { get; set; }

    public bool IsStrong { get; set; }

    public int Score { get; set; }

    public string StrengthLabel { get; set; } = string.Empty;

    public PasswordEvaluationCriteriaDto Criteria { get; set; } = new();

    public List<string> Errors { get; set; } = [];

    public string Message { get; set; } = string.Empty;
}

public sealed class LogoutRequest
{
    public int? UserId { get; set; }

    public string? Email { get; set; }
}

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

public sealed class GoogleLoginRequest
{
    public string? IdToken { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? ProfileImage { get; set; }
}

public sealed class MaintenanceStatusResponse
{
    public bool IsMaintenanceMode { get; set; }
    public string Message { get; set; } = string.Empty;
}
