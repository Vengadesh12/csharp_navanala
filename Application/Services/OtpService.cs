using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using MyBackend.Application.Interfaces;

namespace MyBackend.Application.Services
{
    /// <summary>
    /// In-memory thread-safe OTP manager with rate limiting and automatic expiration.
    /// </summary>
    public class OtpService : IOtpService
    {
        private class OtpRecord
        {
            public string Code { get; set; } = string.Empty;
            public DateTime ExpiryTime { get; set; }
            public int FailedAttempts { get; set; }
        }

        private readonly ConcurrentDictionary<string, OtpRecord> _otpStore = new(StringComparer.OrdinalIgnoreCase);
        private const int MaxAttempts = 5;

        public string GenerateOtp(string email, int expiryMinutes = 10)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            
            // Cryptographically secure 6-digit number [100000..999999]
            var otpNumber = RandomNumberGenerator.GetInt32(100000, 1000000);
            var otpCode = otpNumber.ToString();

            var record = new OtpRecord
            {
                Code = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes),
                FailedAttempts = 0
            };

            _otpStore[normalizedEmail] = record;
            return otpCode;
        }

        public bool ValidateOtp(string email, string code, out string? errorMessage)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var cleanCode = code?.Trim() ?? string.Empty;

            if (!_otpStore.TryGetValue(normalizedEmail, out var record))
            {
                errorMessage = "No active OTP request found for this email. Please request a new OTP.";
                return false;
            }

            if (DateTime.UtcNow > record.ExpiryTime)
            {
                _otpStore.TryRemove(normalizedEmail, out _);
                errorMessage = "This OTP has expired. Please request a new OTP.";
                return false;
            }

            if (record.FailedAttempts >= MaxAttempts)
            {
                _otpStore.TryRemove(normalizedEmail, out _);
                errorMessage = "Too many incorrect OTP attempts. Please request a fresh OTP.";
                return false;
            }

            if (!string.Equals(record.Code, cleanCode, StringComparison.Ordinal))
            {
                record.FailedAttempts++;
                var remaining = MaxAttempts - record.FailedAttempts;
                errorMessage = $"Invalid OTP code. {remaining} attempt(s) remaining.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        public bool ConsumeOtp(string email, string code, out string? errorMessage)
        {
            if (!ValidateOtp(email, code, out errorMessage))
            {
                return false;
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            _otpStore.TryRemove(normalizedEmail, out _);
            return true;
        }
    }
}
