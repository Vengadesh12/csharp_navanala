namespace MyBackend.Application.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp(string email, int expiryMinutes = 10);
        bool ValidateOtp(string email, string code, out string? errorMessage);
        bool ConsumeOtp(string email, string code, out string? errorMessage);
    }
}
