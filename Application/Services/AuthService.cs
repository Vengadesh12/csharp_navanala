using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyBackend.Application.Common.Validators;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IEmailService emailService,
            IOtpService otpService,
            IJwtService jwtService,
            IPasswordHasher<User> passwordHasher,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailService = emailService;
            _otpService = otpService;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public AuthService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IEmailService emailService,
            IOtpService otpService,
            IJwtService jwtService,
            ILogger<AuthService> logger)
            : this(unitOfWork.Users, unitOfWork, configuration, emailService, otpService, jwtService, new PasswordHasher<User>(), logger)
        {
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            var loginDetails = await _userRepository.GetLoginUserDetailsByEmailAsync(request.Email);

            if (loginDetails is null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (loginDetails.DeletedFlag == 0)
            {
                throw new UnauthorizedAccessException("This account has been deactivated. Please contact your administrator.");
            }

            var user = loginDetails.ToUser();
            await EnsureMaintenanceAccessAllowedAsync(user);

            if (string.IsNullOrWhiteSpace(loginDetails.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, loginDetails.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                try
                {
                    var updatedHash = _passwordHasher.HashPassword(user, request.Password);
                    await _userRepository.UpdatePasswordHashAsync(user.Id, updatedHash);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to re-hash password on login for user {Email}", user.Email);
                }
            }

            // Check if Two-Factor Authentication is active
            var twoFactorVal = await _unitOfWork.SystemSettings.GetSettingValueAsync("two_factor_auth");
            bool isTwoFactorEnabled = string.Equals(twoFactorVal?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

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
                        Id = user.Id,
                        Email = user.Email,
                        Name = user.Name,
                        Phone = user.Phone,
                        Age = user.Age,
                        Address = user.Address,
                        ProfileImage = user.ProfileImage,
                        RoleId = user.RoleId,
                        RoleName = loginDetails.RoleName,
                        DesignationName = loginDetails.DesignationName,
                        DepartmentName = loginDetails.DepartmentName,
                        Permissions = loginDetails.GetPermissions(),
                        MenuNames = loginDetails.GetMenuNames()
                    }
                };
            }

            var authData = await BuildAuthUserDataAsync(loginDetails);

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
                await _unitOfWork.AuditLogs.CreateAuditLogAsync(
                    action: "User Login",
                    module: "Auth",
                    performedBy: user.Name,
                    details: $"User logged in successfully from IP {clientIp}",
                    ipAddress: clientIp,
                    status: "Success"
                );
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
            var loginDetails = await _userRepository.GetLoginUserDetailsByEmailAsync(request.Email);

            if (loginDetails is null || loginDetails.DeletedFlag == 0)
            {
                throw new KeyNotFoundException("User account not found or deactivated.");
            }

            var user = loginDetails.ToUser();
            await EnsureMaintenanceAccessAllowedAsync(user);

            if (!_otpService.ConsumeOtp(user.Email, request.Otp, out var errorMessage))
            {
                throw new InvalidOperationException(errorMessage ?? "Invalid or expired 2FA verification code.");
            }

            var authData = await BuildAuthUserDataAsync(loginDetails);

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
                await _unitOfWork.AuditLogs.CreateAuditLogAsync(
                    action: "2FA Login Verification",
                    module: "Auth",
                    performedBy: user.Name,
                    details: $"User 2FA verified and signed in from IP {clientIp}",
                    ipAddress: clientIp,
                    status: "Success"
                );
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

        public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request, string? ipAddress = null, string? userAgent = null)
        {
            if (!string.IsNullOrWhiteSpace(request.IdToken))
            {
                try
                {
                    var (tokenEmail, tokenName, tokenPicture) = _jwtService.ReadTokenPayload(request.IdToken);
                    if (!string.IsNullOrWhiteSpace(tokenEmail))
                    {
                        request.Email = tokenEmail;
                    }
                    if (!string.IsNullOrWhiteSpace(tokenName) && string.IsNullOrWhiteSpace(request.Name))
                    {
                        request.Name = tokenName;
                    }
                    if (!string.IsNullOrWhiteSpace(tokenPicture) && string.IsNullOrWhiteSpace(request.ProfileImage))
                    {
                        request.ProfileImage = tokenPicture;
                    }
                }
                catch
                {
                    // Fallback gracefully if token parsing encounters malformed data
                }
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("A valid Google email address is required.");
            }

            var loginDetails = await _userRepository.GetLoginUserDetailsByEmailAsync(request.Email);
            if (loginDetails == null)
            {
                throw new UnauthorizedAccessException($"No registered workspace account found for '{request.Email}'. Please contact your administrator to create your account or submit an access request.");
            }

            if (loginDetails.DeletedFlag == 0)
            {
                throw new UnauthorizedAccessException("This account has been deactivated. Please contact your administrator.");
            }

            var user = loginDetails.ToUser();
            await EnsureMaintenanceAccessAllowedAsync(user);

            if (string.IsNullOrWhiteSpace(user.ProfileImage) && !string.IsNullOrWhiteSpace(request.ProfileImage))
            {
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    existingUser.ProfileImage = request.ProfileImage;
                    user.ProfileImage = request.ProfileImage;
                    loginDetails.ProfileImage = request.ProfileImage;
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            var authData = await BuildAuthUserDataAsync(loginDetails);

            // Record login session with IP Address and timestamp
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
                await _unitOfWork.AuditLogs.CreateAuditLogAsync(
                    action: "Google OAuth Login",
                    module: "Auth",
                    performedBy: user.Name,
                    details: $"User signed in via Google OAuth from IP {clientIp}",
                    ipAddress: clientIp,
                    status: "Success"
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record Google login session audit for user {Email}", user.Email);
            }

            return new LoginResponse
            {
                Success = true,
                RequiresTwoFactor = false,
                Message = "Signed in with Google successfully.",
                Data = authData
            };
        }

        public async Task<MessageResponse> LogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null)
        {
            var clientIp = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress;

            User? user = null;
            if (userId > 0)
            {
                user = await _userRepository.GetUserByIdAsync(userId);
            }
            if (user == null && !string.IsNullOrWhiteSpace(email))
            {
                user = await _userRepository.GetByEmailAsync(email);
            }

            // Record logout time and deactivate session in user_sessions table
            await _unitOfWork.Sessions.RecordLogoutAsync(user?.Id ?? userId, clientIp, sessionToken, email ?? user?.Email);

            // Record audit log for logout
            try
            {
                await _unitOfWork.AuditLogs.CreateAuditLogAsync(
                    action: "User Logout",
                    module: "Auth",
                    performedBy: user?.Name ?? $"User ID: {userId}",
                    details: $"User logged out at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC from IP {clientIp}",
                    ipAddress: clientIp,
                    status: "Success"
                );
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
            var user = await _userRepository.GetByEmailAsync(request.Email);

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
            var user = await _userRepository.GetByEmailAsync(request.Email);

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

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null || user.DeletedFlag == 0)
            {
                throw new KeyNotFoundException("User account not found or has been deactivated.");
            }

            var newHash = _passwordHasher.HashPassword(user, request.NewPassword);
            await _userRepository.UpdatePasswordHashAsync(user.Id, newHash);

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
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user is null || user.DeletedFlag == 0)
            {
                throw new UnauthorizedAccessException("User not found or deactivated.");
            }

            var permissions = await _userRepository.GetUserPermissionKeysAsync(userId);
            return new CurrentUserPermissionsResponse { Permissions = permissions };
        }

        public async Task<List<UserSessionDto>> GetUserSessionsAsync(int userId, int limit = 50)
        {
            var sessions = await _unitOfWork.Sessions.GetUserSessionsAsync(userId, limit);
            return sessions.ToDtoList();
        }

        public async Task<List<UserSessionDto>> GetAllRecentSessionsAsync(int limit = 100)
        {
            var sessions = await _unitOfWork.Sessions.GetAllRecentSessionsAsync(limit);
            return sessions.ToDtoList();
        }

        private Task<AuthUserData> BuildAuthUserDataAsync(UserLoginDetails loginDetails)
        {
            var user = loginDetails.ToUser();
            return BuildAuthUserDataAsync(
                user,
                loginDetails.RoleName,
                loginDetails.DesignationName,
                loginDetails.DepartmentName,
                loginDetails.GetPermissions(),
                loginDetails.GetMenuNames());
        }

        private async Task<AuthUserData> BuildAuthUserDataAsync(
            User user,
            string? roleName = null,
            string? designationName = null,
            string? departmentName = null,
            List<string>? initialPermissions = null,
            List<string>? initialMenuNames = null)
        {
            if (string.IsNullOrWhiteSpace(roleName) && user.RoleId.HasValue)
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId.Value);
                roleName = role?.Name;
            }

            if (string.IsNullOrWhiteSpace(designationName) && user.DesignationId.HasValue)
            {
                var des = await _unitOfWork.Designations.GetByIdAsync(user.DesignationId.Value);
                designationName = des?.Name;
                if (string.IsNullOrWhiteSpace(departmentName) && des?.DepartmentId.HasValue == true)
                {
                    var dept = await _unitOfWork.Departments.GetByIdAsync(des.DepartmentId.Value);
                    departmentName = dept?.Name;
                }
            }
            else if (string.IsNullOrWhiteSpace(departmentName) && user.DesignationId.HasValue)
            {
                var des = await _unitOfWork.Designations.GetByIdAsync(user.DesignationId.Value);
                if (des?.DepartmentId.HasValue == true)
                {
                    var dept = await _unitOfWork.Departments.GetByIdAsync(des.DepartmentId.Value);
                    departmentName = dept?.Name;
                }
            }

            var permissions = (initialPermissions != null && initialPermissions.Count > 0)
                ? initialPermissions
                : await _userRepository.GetUserPermissionKeysAsync(user.Id, user.RoleId, user.DesignationId);

            List<Menu> menus = [];
            try
            {
                if (user.RoleId == 2)
                {
                    menus = await _unitOfWork.Menus.GetAllActiveMenusAsync();
                }
                else
                {
                    menus = await _unitOfWork.Menus.GetUserMenusAsync(user.RoleId ?? 0, user.DesignationId ?? 0, user.Id);
                }
            }
            catch
            {
            }

            var menuNames = (initialMenuNames != null && initialMenuNames.Count > 0)
                ? initialMenuNames
                : menus.Select(m => m.Label).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().ToList();

            var tokenString = _jwtService.GenerateToken(user, roleName, permissions);

            return new AuthUserData
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ProfileImage = user.ProfileImage,
                RoleId = user.RoleId,
                RoleName = roleName,
                DepartmentName = departmentName,
                DesignationName = designationName,
                Permissions = permissions,
                Menus = menus.ToDtoList(),
                MenuNames = menuNames,
                Token = tokenString,
                Phone = user.Phone,
                Age = user.Age,
                Address = user.Address,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<MaintenanceStatusResponse> GetMaintenanceStatusAsync()
        {
            var maintenanceVal = await _unitOfWork.SystemSettings.GetSettingValueAsync("maintenance_mode");
            bool isMaintenance = string.Equals(maintenanceVal?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

            return new MaintenanceStatusResponse
            {
                IsMaintenanceMode = isMaintenance,
                Message = isMaintenance
                    ? "This website is under maintenance, please come again later."
                    : string.Empty
            };
        }

        private async Task EnsureMaintenanceAccessAllowedAsync(User user)
        {
            var maintenanceVal = await _unitOfWork.SystemSettings.GetSettingValueAsync("maintenance_mode");
            bool isMaintenance = string.Equals(maintenanceVal?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

            if (isMaintenance)
            {
                bool isAdmin = await IsAdminUserAsync(user);
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("This website is under maintenance, please come again later.");
                }
            }
        }

        private async Task<bool> IsAdminUserAsync(User user)
        {
            if (user.RoleId == 2) return true;
            if (user.RoleId.HasValue)
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId.Value);
                if (role != null && (role.Id == 2 || role.Name.Contains("admin", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
