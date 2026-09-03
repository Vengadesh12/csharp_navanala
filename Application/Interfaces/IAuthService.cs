using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null);

        Task<LoginResponse> Verify2FaLoginAsync(Verify2FaLoginRequest request, string? ipAddress = null, string? userAgent = null);

        Task<MessageResponse> LogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null);

        Task<MessageResponse> Resend2FaOtpAsync(Resend2FaOtpRequest request);

        Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request);

        MessageResponse VerifyOtp(VerifyOtpRequest request);

        Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request);

        EvaluatePasswordResponse EvaluatePassword(EvaluatePasswordRequest request);

        Task<CurrentUserPermissionsResponse> GetUserPermissionsAsync(int userId);

        Task<List<UserSessionDto>> GetUserSessionsAsync(int userId, int limit = 50);

        Task<List<UserSessionDto>> GetAllRecentSessionsAsync(int limit = 100);

        Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request, string? ipAddress = null, string? userAgent = null);

        Task<MaintenanceStatusResponse> GetMaintenanceStatusAsync();
    }
}
