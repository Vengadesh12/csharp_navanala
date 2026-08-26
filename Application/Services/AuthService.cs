using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyBackend.Application.Common.Interfaces;
using MyBackend.Application.Common.Validators;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Configuration;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    /// <summary>
    /// Implements user authentication, 2FA validation, OTP recovery, login/logout session recording with IP tracking, and real-time password evaluation using repositories.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly ILogger<AuthService> _logger;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IEmailService emailService,
            IOtpService otpService,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailService = emailService;
            _otpService = otpService;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (user.DeletedFlag == 0)
            {
                throw new UnauthorizedAccessException("This account has been deactivated. Please contact your administrator.");
            }

            bool passwordMatches = false;

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                var initialHash = _passwordHasher.HashPassword(user, request.Password);
                await _unitOfWork.Users.UpdatePasswordHashAsync(user.Id, initialHash);
                passwordMatches = true;
            }
            else
            {
                try
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                    if (result != PasswordVerificationResult.Failed)
                    {
                        passwordMatches = true;
                    }
                }
                catch
                {
                    // Fallback
                }

                if (!passwordMatches && (string.Equals(user.PasswordHash, request.Password, StringComparison.Ordinal) ||
                    string.Equals(user.PasswordHash?.Trim(), request.Password?.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    passwordMatches = true;
                    try
                    {
                        var newHash = _passwordHasher.HashPassword(user, request.Password!);
                        await _unitOfWork.Users.UpdatePasswordHashAsync(user.Id, newHash);
                    }
                    catch
                    {
                    }
                }
            }

            if (!passwordMatches)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Check if Two-Factor Authentication is active
            var twoFactorSetting = await _unitOfWork.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "two_factor_auth");

            bool isTwoFactorEnabled = twoFactorSetting != null &&
                string.Equals(twoFactorSetting.SettingValue?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

            if (isTwoFactorEnabled)
            {
                var otpCode = _otpService.GenerateOtp(user.Email, expiryMinutes: 10);
                try
                {
                    await _emailService.SendTwoFactorOtpEmailAsync(user.Email, user.Name, otpCode, expiryMinutes: 10);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispatch 2FA OTP email to {Email}", user.Email);
                }

                return new LoginResponse
                {
                    Success = true,
                    RequiresTwoFactor = true,
                    Message = "Two-Factor Authentication is enabled. A 6-digit verification code has been sent to your registered email.",
                    Data = new AuthUserData
                    {
                        Email = user.Email,
                        Name = user.Name
                    }
                };
            }

            var authData = await BuildAuthUserDataAsync(user);

            // Record login session with IP Address and timestamp in PostgreSQL
            try
            {
                var clientIp = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress;
                await _unitOfWork.Sessions.RecordLoginAsync(
                    userId: user.Id,
                    email: user.Email,
                    userName: user.Name,
                    ipAddress: clientIp,
                    userAgent: userAgent,
                    sessionToken: authData.Token
                );

                // Write Audit Log
                await _unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
                {
                    Action = "User Login",
                    Module = "Auth",
                    PerformedBy = user.Name,
                    Details = $"User logged in successfully from IP {clientIp}",
                    IpAddress = clientIp,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    DeletedFlag = 1
                });
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record login session audit for user {Email}", user.Email);
            }

            return new LoginResponse
            {
                Success = true,
                RequiresTwoFactor = false,
                Message = "Login successful.",
                Data = authData
            };
        }

        public async Task<LoginResponse> Verify2FaLoginAsync(Verify2FaLoginRequest request, string? ipAddress = null, string? userAgent = null)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user is null || user.DeletedFlag == 0)
            {
                throw new KeyNotFoundException("User account not found or deactivated.");
            }

            if (!_otpService.ConsumeOtp(user.Email, request.Otp, out var errorMessage))
            {
                throw new InvalidOperationException(errorMessage ?? "Invalid or expired 2FA verification code.");
            }

            var authData = await BuildAuthUserDataAsync(user);

            // Record login session with IP Address and timestamp in PostgreSQL
            try
            {
                var clientIp = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress;
                await _unitOfWork.Sessions.RecordLoginAsync(
                    userId: user.Id,
                    email: user.Email,
                    userName: user.Name,
                    ipAddress: clientIp,
                    userAgent: userAgent,
                    sessionToken: authData.Token
                );

                // Write Audit Log
                await _unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
                {
                    Action = "2FA Login Verification",
                    Module = "Auth",
                    PerformedBy = user.Name,
                    Details = $"User 2FA verified and signed in from IP {clientIp}",
                    IpAddress = clientIp,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    DeletedFlag = 1
                });
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record 2FA login session audit for user {Email}", user.Email);
            }

            return new LoginResponse
            {
                Success = true,
                RequiresTwoFactor = false,
                Message = "Two-Factor Authentication verified successfully.",
                Data = authData
            };
        }

        public async Task<MessageResponse> LogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null)
        {
            var clientIp = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress;

            User? user = null;
            if (userId > 0)
            {
                user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            }
            if (user == null && !string.IsNullOrWhiteSpace(email))
            {
                user = await _unitOfWork.Users.GetByEmailAsync(email);
            }

            // Record logout time and deactivate session in user_sessions table
            await _unitOfWork.Sessions.RecordLogoutAsync(user?.Id ?? userId, clientIp, sessionToken, email ?? user?.Email);

            // Record audit log for logout
            try
            {
                await _unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
                {
                    Action = "User Logout",
                    Module = "Auth",
                    PerformedBy = user?.Name ?? $"User ID: {userId}",
                    Details = $"User logged out at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC from IP {clientIp}",
                    IpAddress = clientIp,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    DeletedFlag = 1
                });
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record logout audit for user {UserId}", userId);
            }

            return new MessageResponse
            {
                Success = true,
                Message = "Logged out successfully. Session ended."
            };
        }

        public async Task<MessageResponse> Resend2FaOtpAsync(Resend2FaOtpRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user is null || user.DeletedFlag == 0)
            {
                throw new KeyNotFoundException("No active account found with this email address.");
            }

            var otpCode = _otpService.GenerateOtp(user.Email, expiryMinutes: 10);
            await _emailService.SendTwoFactorOtpEmailAsync(user.Email, user.Name, otpCode, expiryMinutes: 10);

            return new MessageResponse
            {
                Success = true,
                Message = "A new 6-digit 2FA verification code has been sent to your email."
            };
        }

        public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user is null || user.DeletedFlag == 0)
            {
                throw new KeyNotFoundException("No active account found with this email address.");
            }

            var otpCode = _otpService.GenerateOtp(user.Email, expiryMinutes: 10);
            await _emailService.SendPasswordResetOtpEmailAsync(user.Email, user.Name, otpCode, expiryMinutes: 10);

            return new MessageResponse
            {
                Success = true,
                Message = "A 6-digit verification OTP has been sent to your email address."
            };
        }

        public MessageResponse VerifyOtp(VerifyOtpRequest request)
        {
            if (!_otpService.ValidateOtp(request.Email, request.Otp, out var errorMessage))
            {
                throw new InvalidOperationException(errorMessage ?? "Invalid or expired OTP code.");
            }

            return new MessageResponse
            {
                Success = true,
                Message = "OTP verified successfully. You may now enter your new strong password."
            };
        }

        public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.ConfirmPassword) &&
                !string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            {
                throw new ArgumentException("New password and confirmation password do not match.");
            }

            var (isValid, errors) = PasswordValidator.Validate(request.NewPassword);
            if (!isValid)
            {
                throw new ArgumentException(errors.Count > 0 ? errors[0] : "Password does not meet strong security requirements.");
            }

            if (!_otpService.ConsumeOtp(request.Email, request.Otp, out var otpError))
            {
                throw new InvalidOperationException(otpError ?? "Invalid or expired OTP code.");
            }

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user is null || user.DeletedFlag == 0)
            {
                throw new KeyNotFoundException("User account not found or has been deactivated.");
            }

            var newHash = _passwordHasher.HashPassword(user, request.NewPassword);
            await _unitOfWork.Users.UpdatePasswordHashAsync(user.Id, newHash);

            try
            {
                await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send password changed notification email to {Email}", user.Email);
            }

            return new MessageResponse
            {
                Success = true,
                Message = "Your password has been reset successfully. You can now log in with your new credentials."
            };
        }

        public EvaluatePasswordResponse EvaluatePassword(EvaluatePasswordRequest request)
        {
            var eval = PasswordValidator.Evaluate(request?.Password);

            return new EvaluatePasswordResponse
            {
                IsValid = eval.IsValid,
                IsStrong = eval.IsStrong,
                Score = eval.Score,
                StrengthLabel = eval.StrengthLabel,
                Criteria = new PasswordEvaluationCriteriaDto
                {
                    MinLength = eval.MinLength,
                    HasUpper = eval.HasUpper,
                    HasLower = eval.HasLower,
                    HasNumber = eval.HasNumber,
                    HasSpecial = eval.HasSpecial
                },
                Errors = eval.Errors,
                Message = eval.IsValid
                    ? "Password satisfies strong security requirements."
                    : (eval.Errors.Count > 0 ? eval.Errors[0] : "Password does not meet strong security requirements.")
            };
        }

        public async Task<CurrentUserPermissionsResponse> GetUserPermissionsAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);

            if (user is null || user.DeletedFlag == 0)
            {
                throw new UnauthorizedAccessException("User not found or deactivated.");
            }

            var permissions = await _unitOfWork.Users.GetUserPermissionKeysAsync(userId);
            return new CurrentUserPermissionsResponse { Permissions = permissions };
        }

        public async Task<List<UserSession>> GetUserSessionsAsync(int userId, int limit = 50)
        {
            return await _unitOfWork.Sessions.GetUserSessionsAsync(userId, limit);
        }

        public async Task<List<UserSession>> GetAllRecentSessionsAsync(int limit = 100)
        {
            return await _unitOfWork.Sessions.GetAllRecentSessionsAsync(limit);
        }

        private async Task<AuthUserData> BuildAuthUserDataAsync(User user)
        {
            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId.Value);
                roleName = role?.Name;
            }

            var permissions = await _unitOfWork.Users.GetUserPermissionKeysAsync(user.Id);

            List<Menu> menus = [];
            try
            {
                if (user.RoleId == 2)
                {
                    menus = (await _unitOfWork.Menus.FindAsync(m => m.DeletedFlag == 1))
                        .OrderBy(m => m.OrderIndex)
                        .ThenBy(m => m.Id)
                        .ToList();
                }
                else if (user.RoleId.HasValue)
                {
                    menus = (await _unitOfWork.Menus.FindAsync(m => m.DeletedFlag == 1 &&
                        (string.IsNullOrEmpty(m.PermissionKey) || permissions.Contains(m.PermissionKey))))
                        .OrderBy(m => m.OrderIndex)
                        .ThenBy(m => m.Id)
                        .ToList();
                }
                else
                {
                    menus = (await _unitOfWork.Menus.FindAsync(m => m.DeletedFlag == 1 && string.IsNullOrEmpty(m.PermissionKey)))
                        .OrderBy(m => m.OrderIndex)
                        .ThenBy(m => m.Id)
                        .ToList();
                }
            }
            catch
            {
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.RoleId?.ToString() ?? string.Empty)
            };

            var jwtKey = !string.IsNullOrWhiteSpace(_configuration["Jwt:Key"]) ? _configuration["Jwt:Key"]! : Config.JwtKey;
            var jwtIssuer = !string.IsNullOrWhiteSpace(_configuration["Jwt:Issuer"]) ? _configuration["Jwt:Issuer"]! : Config.JwtIssuer;
            var jwtAudience = !string.IsNullOrWhiteSpace(_configuration["Jwt:Audience"]) ? _configuration["Jwt:Audience"]! : Config.JwtAudience;
            var jwtExpires = _configuration.GetValue<int>("Jwt:ExpiresMinutes", Config.JwtExpiresMinutes);

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpires),
                signingCredentials: credentials);

            return new AuthUserData
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = roleName,
                Permissions = permissions,
                Menus = menus,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }
    }
}
