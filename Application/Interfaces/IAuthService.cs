using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for user authentication, 2FA verification, OTP recovery, session logging, and password strength evaluation.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates user credentials, tracks login session with IP address and timestamp, and returns JWT token or initiates 2FA challenge.
        /// </summary>
        Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Verifies 2FA OTP code, records active session with IP address, and completes the sign-in session.
        /// </summary>
        Task<LoginResponse> Verify2FaLoginAsync(Verify2FaLoginRequest request, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Records user logout timestamp, deactivates active session, and logs audit trail.
        /// </summary>
        Task<MessageResponse> LogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null);

        /// <summary>
        /// Re-sends a 2FA OTP security code to the registered email.
        /// </summary>
        Task<MessageResponse> Resend2FaOtpAsync(Resend2FaOtpRequest request);

        /// <summary>
        /// Initiates password recovery by dispatching a verification OTP.
        /// </summary>
        Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request);

        /// <summary>
        /// Validates OTP code before password reset.
        /// </summary>
        MessageResponse VerifyOtp(VerifyOtpRequest request);

        /// <summary>
        /// Completes password reset using verified OTP with strong password enforcement.
        /// </summary>
        Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request);

        /// <summary>
        /// Evaluates password complexity and strength against security policies in real-time.
        /// </summary>
        EvaluatePasswordResponse EvaluatePassword(EvaluatePasswordRequest request);

        /// <summary>
        /// Resolves granted RBAC permissions for the authenticated user.
        /// </summary>
        Task<CurrentUserPermissionsResponse> GetUserPermissionsAsync(int userId);

        /// <summary>
        /// Retrieves the session login/logout history for a specific user.
        /// </summary>
        Task<List<UserSessionDto>> GetUserSessionsAsync(int userId, int limit = 50);

        /// <summary>
        /// Retrieves all recent user login and logout sessions with IP addresses.
        /// </summary>
        Task<List<UserSessionDto>> GetAllRecentSessionsAsync(int limit = 100);

        /// <summary>
        /// Authenticates user credentials via Google OAuth credential, tracks session with IP address, and returns JWT token.
        /// </summary>
        Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Retrieves the current system maintenance mode status.
        /// </summary>
        Task<MaintenanceStatusResponse> GetMaintenanceStatusAsync();
    }
}
