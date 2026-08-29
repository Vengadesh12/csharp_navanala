namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for sending transactional emails (credentials, OTP codes, alerts).
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendWelcomeUserEmailAsync(string recipientEmail, string recipientName, string plainPassword);
        Task SendPasswordResetOtpEmailAsync(string recipientEmail, string recipientName, string otpCode, int expiryMinutes = 10);
        Task SendTwoFactorOtpEmailAsync(string recipientEmail, string recipientName, string otpCode, int expiryMinutes = 10);
        Task SendPasswordChangedNotificationAsync(string recipientEmail, string recipientName);
    }
}
