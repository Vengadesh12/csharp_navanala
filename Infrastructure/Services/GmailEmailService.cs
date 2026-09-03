using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyBackend.Application.Interfaces;
using MyBackend.Configuration;
using EmailSettings = MyBackend.Application.Common.Models.EmailSettings;

namespace MyBackend.Infrastructure.Services
{
    public class GmailEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<GmailEmailService> _logger;

        public GmailEmailService(IOptions<EmailSettings> settings, ILogger<GmailEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var senderEmail = !string.IsNullOrWhiteSpace(_settings.SenderEmail) && !_settings.SenderEmail.Contains("your-email@gmail.com")
                ? _settings.SenderEmail
                : Config.SenderEmail;

            var appPassword = !string.IsNullOrWhiteSpace(_settings.AppPassword) && !_settings.AppPassword.Contains("your-16-digit-app-password")
                ? _settings.AppPassword
                : Config.GmailPassword;

            var smtpServer = !string.IsNullOrWhiteSpace(_settings.SmtpServer) ? _settings.SmtpServer : Config.SmtpServer;
            var port = _settings.Port > 0 ? _settings.Port : Config.SmtpPort;
            var senderName = !string.IsNullOrWhiteSpace(_settings.SenderName) ? _settings.SenderName : Config.SenderName;
            var enableSsl = _settings.EnableSsl;

            if (string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(appPassword) ||
                senderEmail.Contains("your-email@gmail.com"))
            {
                _logger.LogWarning("Email delivery skipped: Gmail credentials are not configured in Config.cs or appsettings.json.");
                return;
            }

            try
            {
                var cleanAppPassword = appPassword.Replace(" ", "").Trim();

                using var client = new SmtpClient(smtpServer, port)
                {
                    Credentials = new NetworkCredential(senderEmail.Trim(), cleanAppPassword),
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail.Trim(), senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email '{Subject}' sent successfully to {Email}", subject, toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendWelcomeUserEmailAsync(string recipientEmail, string recipientName, string plainPassword)
        {
            var subject = "Welcome to Workspace - Your Account Credentials";
            var body = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <style>
                    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }
                    .container { max-width: 560px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.06); border: 1px solid #e2e8f0; }
                    .header { background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); color: #ffffff; padding: 32px 24px; text-align: center; }
                    .header h1 { margin: 0; font-size: 22px; font-weight: 700; letter-spacing: -0.5px; }
                    .content { padding: 32px 28px; line-height: 1.6; }
                    .credentials-box { background: #f8fafc; border: 1px solid #e2e8f0; border-left: 4px solid #4f46e5; border-radius: 8px; padding: 20px; margin: 24px 0; }
                    .credential-row { margin-bottom: 12px; font-size: 15px; }
                    .credential-row:last-child { margin-bottom: 0; }
                    .credential-label { font-weight: 600; color: #64748b; display: inline-block; width: 110px; }
                    .credential-value { font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace; font-weight: 700; color: #0f172a; background: #e0e7ff; padding: 3px 8px; border-radius: 4px; display: inline-block; }
                    .btn-container { text-align: center; margin: 28px 0 16px; }
                    .button { display: inline-block; background: #4f46e5; color: #ffffff !important; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px; box-shadow: 0 2px 6px rgba(79, 70, 229, 0.3); }
                    .notice { font-size: 13px; color: #64748b; background: #fffbeb; border: 1px solid #fef3c7; padding: 12px 16px; border-radius: 6px; margin-top: 20px; }
                    .footer { background: #f8fafc; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Welcome to Workspace</h1>
                    </div>
                    <div class="content">
                        <p>Hello <strong>{{recipientName}}</strong>,</p>
                        <p>Your user account has been successfully created. You can now log into the portal using the credentials below:</p>
                        
                        <div class="credentials-box">
                            <div class="credential-row">
                                <span class="credential-label">Email ID:</span>
                                <span class="credential-value">{{recipientEmail}}</span>
                            </div>
                            <div class="credential-row">
                                <span class="credential-label">Password:</span>
                                <span class="credential-value">{{plainPassword}}</span>
                            </div>
                        </div>

                        <div class="notice">
                            <strong>Security Note:</strong> For security purposes, please log into your account and change your password.
                        </div>

                        <div class="btn-container">
                            <a href="http://localhost:5173" class="button">Log In to Workspace</a>
                        </div>
                    </div>
                    <div class="footer">
                        <p>This is an automated notification. Please do not reply directly to this email.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendPasswordResetOtpEmailAsync(string recipientEmail, string recipientName, string otpCode, int expiryMinutes = 10)
        {
            var subject = $"Your Password Reset OTP: {otpCode}";
            var body = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <style>
                    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }
                    .container { max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.06); border: 1px solid #e2e8f0; }
                    .header { background: linear-gradient(135deg, #0284c7 0%, #4f46e5 100%); color: #ffffff; padding: 30px 24px; text-align: center; }
                    .header h1 { margin: 0; font-size: 20px; font-weight: 700; }
                    .content { padding: 32px 28px; line-height: 1.6; }
                    .otp-box { text-align: center; margin: 26px 0; background: #f8fafc; border: 2px dashed #cbd5e1; border-radius: 12px; padding: 22px 16px; }
                    .otp-label { font-size: 13px; font-weight: 600; text-transform: uppercase; color: #64748b; letter-spacing: 1px; margin-bottom: 8px; }
                    .otp-code { font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace; font-size: 36px; font-weight: 800; color: #4f46e5; letter-spacing: 8px; margin: 0; }
                    .expiry-badge { display: inline-block; margin-top: 10px; background: #fee2e2; color: #b91c1c; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600; }
                    .notice { font-size: 13px; color: #64748b; background: #f8fafc; border: 1px solid #e2e8f0; padding: 12px 16px; border-radius: 6px; margin-top: 20px; }
                    .footer { background: #f8fafc; padding: 18px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Password Reset Verification</h1>
                    </div>
                    <div class="content">
                        <p>Hello <strong>{{recipientName}}</strong>,</p>
                        <p>We received a request to reset your workspace account password. Use the verification OTP below to complete the reset process:</p>
                        
                        <div class="otp-box">
                            <div class="otp-label">Verification Code (OTP)</div>
                            <div class="otp-code">{{otpCode}}</div>
                            <div class="expiry-badge">&#9201; Valid for {{expiryMinutes}} minutes</div>
                        </div>

                        <div class="notice">
                            <strong>Security Reminder:</strong> Never share this OTP with anyone. Our administrators will never ask for your OTP. If you did not request a password reset, you can safely ignore this email.
                        </div>
                    </div>
                    <div class="footer">
                        <p>This is an automated security verification message from Workspace RBAC.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendTwoFactorOtpEmailAsync(string recipientEmail, string recipientName, string otpCode, int expiryMinutes = 10)
        {
            var subject = $"Your 2FA Security Login Code: {otpCode}";
            var body = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <style>
                    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }
                    .container { max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.06); border: 1px solid #e2e8f0; }
                    .header { background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); color: #ffffff; padding: 30px 24px; text-align: center; }
                    .header h1 { margin: 0; font-size: 20px; font-weight: 700; }
                    .content { padding: 32px 28px; line-height: 1.6; }
                    .otp-box { text-align: center; margin: 26px 0; background: #f8fafc; border: 2px dashed #6366f1; border-radius: 12px; padding: 22px 16px; }
                    .otp-label { font-size: 13px; font-weight: 600; text-transform: uppercase; color: #64748b; letter-spacing: 1px; margin-bottom: 8px; }
                    .otp-code { font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace; font-size: 38px; font-weight: 800; color: #4f46e5; letter-spacing: 8px; margin: 0; }
                    .expiry-badge { display: inline-block; margin-top: 10px; background: #fee2e2; color: #b91c1c; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600; }
                    .notice { font-size: 13px; color: #64748b; background: #f8fafc; border: 1px solid #e2e8f0; padding: 12px 16px; border-radius: 6px; margin-top: 20px; }
                    .footer { background: #f8fafc; padding: 18px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Two-Factor Authentication</h1>
                    </div>
                    <div class="content">
                        <p>Hello <strong>{{recipientName}}</strong>,</p>
                        <p>A login attempt was initiated for your account. Use the 6-digit two-factor verification code below to complete your sign-in:</p>
                        
                        <div class="otp-box">
                            <div class="otp-label">Security Verification Code</div>
                            <div class="otp-code">{{otpCode}}</div>
                            <div class="expiry-badge">&#9201; Valid for {{expiryMinutes}} minutes</div>
                        </div>

                        <div class="notice">
                            <strong>Security Notice:</strong> Do not disclose this verification code to anyone. If you did not initiate this login request, change your password immediately.
                        </div>
                    </div>
                    <div class="footer">
                        <p>Workspace Access Management &bull; Two-Factor Security Guard</p>
                    </div>
                </div>
            </body>
            </html>
            """;

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendPasswordChangedNotificationAsync(string recipientEmail, string recipientName)
        {
            var subject = "Security Alert - Your Password Has Been Changed";
            var body = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <style>
                    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }
                    .container { max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.06); border: 1px solid #e2e8f0; }
                    .header { background: linear-gradient(135deg, #059669 0%, #10b981 100%); color: #ffffff; padding: 30px 24px; text-align: center; }
                    .header h1 { margin: 0; font-size: 20px; font-weight: 700; }
                    .content { padding: 32px 28px; line-height: 1.6; }
                    .alert-box { background: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 8px; padding: 16px; margin: 20px 0; color: #166534; font-size: 14px; }
                    .btn-container { text-align: center; margin: 24px 0 12px; }
                    .button { display: inline-block; background: #059669; color: #ffffff !important; padding: 12px 28px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 14px; }
                    .footer { background: #f8fafc; padding: 18px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Password Successfully Reset</h1>
                    </div>
                    <div class="content">
                        <p>Hello <strong>{{recipientName}}</strong>,</p>
                        <div class="alert-box">
                            &#10004; The password for your account (<strong>{{recipientEmail}}</strong>) was successfully changed.
                        </div>
                        <p>You can now sign in with your new password.</p>
                        <div class="btn-container">
                            <a href="http://localhost:5173" class="button">Go to Login</a>
                        </div>
                        <p style="font-size: 13px; color: #64748b; margin-top: 24px;">
                            If you did not make this change, please contact your workspace administrator immediately to secure your account.
                        </p>
                    </div>
                    <div class="footer">
                        <p>Workspace Security &bull; Automated notification</p>
                    </div>
                </div>
            </body>
            </html>
            """;

            await SendEmailAsync(recipientEmail, subject, body);
        }
    }
}
