using MyBackend.Application.Common.Models;
using Microsoft.Extensions.Configuration;

namespace MyBackend.Configuration
{
    /// <summary>
    /// Centralized application configuration containing PostgreSQL database connection string,
    /// Gmail SMTP credentials, and JWT authorization secrets.
    /// </summary>
    public static class Config
    {
        // =========================================================================
        // 1. Database Connection Configuration
        // =========================================================================
        /// <summary>
        /// PostgreSQL database connection string.
        /// </summary>
         public static string DbConnectionString { get; set; } =
            "Host=ep-fancy-fog-ayv3j9wc.c-5.us-east-2.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_wsEQdcmO49nX;SSL Mode=Require;Trust Server Certificate=true;";
                
                // "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Test;";
        // =========================================================================
        // 2. Gmail / SMTP Configuration & Password
        // =========================================================================
        /// <summary>
        /// SMTP host server address (e.g. smtp.gmail.com).
        /// </summary>
        public static string SmtpServer { get; set; } = "smtp.gmail.com";

        /// <summary>
        /// SMTP port number (587 for TLS / STARTTLS).
        /// </summary>
        public static int SmtpPort { get; set; } = 587;

        /// <summary>
        /// Display sender name in recipient emails.
        /// </summary>
        public static string SenderName { get; set; } = "Workspace Administration";

        /// <summary>
        /// Sender's registered Gmail address.
        /// </summary>
        public static string SenderEmail { get; set; } = "venkikc333@gmail.com";

        /// <summary>
        /// 16-character Google App Password for Gmail SMTP authentication.
        /// </summary>
        public static string GmailPassword { get; set; } = "dznudcfzffnyeqjl";

        /// <summary>
        /// Whether SSL/TLS is enabled for SMTP communication.
        /// </summary>
        public static bool EnableSsl { get; set; } = true;

        // =========================================================================
        // 3. JWT Security Configuration
        // =========================================================================
        /// <summary>
        /// Secret key for HMAC SHA-256 JWT signing.
        /// </summary>
        public static string JwtKey { get; set; } = "change-this-development-key-to-a-long-random-secret-1234567890";

        /// <summary>
        /// JWT Token Issuer identifier.
        /// </summary>
        public static string JwtIssuer { get; set; } = "Userspace";

        /// <summary>
        /// JWT Token Audience identifier.
        /// </summary>
        public static string JwtAudience { get; set; } = "Userspace.Web";

        /// <summary>
        /// JWT Token expiration window in minutes.
        /// </summary>
        public static int JwtExpiresMinutes { get; set; } = 120;

        // =========================================================================
        // Helper Methods
        // =========================================================================

        /// <summary>
        /// Synchronizes and overlays configuration values from IConfiguration / environment variables / appsettings.json if available.
        /// </summary>
        /// <param name="configuration">The application configuration root.</param>
        public static void Load(IConfiguration configuration)
        {
            if (configuration == null) return;

            // Database Connection (Supports DefaultConnection, DATABASE_URL, and Render postgres:// format)
            var connection = configuration.GetConnectionString("DefaultConnection")
                ?? configuration["DATABASE_URL"]
                ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? configuration["ConnectionStrings:DefaultConnection"]
                ?? configuration["DbConnectionString"];

            if (!string.IsNullOrWhiteSpace(connection))
            {
                DbConnectionString = ConvertPostgresUriToNpgsql(connection);
            }

            // Gmail Settings
            var smtp = configuration["EmailSettings:SmtpServer"] ?? Environment.GetEnvironmentVariable("SMTP_SERVER");
            if (!string.IsNullOrWhiteSpace(smtp)) SmtpServer = smtp;

            var portVal = configuration["EmailSettings:Port"] ?? Environment.GetEnvironmentVariable("SMTP_PORT");
            if (int.TryParse(portVal, out var port) && port > 0)
                SmtpPort = port;

            var sName = configuration["EmailSettings:SenderName"] ?? Environment.GetEnvironmentVariable("SENDER_NAME");
            if (!string.IsNullOrWhiteSpace(sName)) SenderName = sName;

            var sEmail = configuration["EmailSettings:SenderEmail"] ?? Environment.GetEnvironmentVariable("SENDER_EMAIL");
            if (!string.IsNullOrWhiteSpace(sEmail)) SenderEmail = sEmail;

            var appPwd = configuration["EmailSettings:AppPassword"] ?? Environment.GetEnvironmentVariable("GMAIL_PASSWORD") ?? Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            if (!string.IsNullOrWhiteSpace(appPwd)) GmailPassword = appPwd;

            var sslVal = configuration["EmailSettings:EnableSsl"] ?? Environment.GetEnvironmentVariable("ENABLE_SSL");
            if (bool.TryParse(sslVal, out var ssl))
                EnableSsl = ssl;

            // JWT Settings
            var key = configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
            if (!string.IsNullOrWhiteSpace(key)) JwtKey = key;

            var issuer = configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER");
            if (!string.IsNullOrWhiteSpace(issuer)) JwtIssuer = issuer;

            var audience = configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            if (!string.IsNullOrWhiteSpace(audience)) JwtAudience = audience;

            var expVal = configuration["Jwt:ExpiresMinutes"] ?? Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES");
            if (int.TryParse(expVal, out var exp) && exp > 0)
                JwtExpiresMinutes = exp;
        }

        private static string ConvertPostgresUriToNpgsql(string connectionString)
        {
            if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(connectionString);
                    var userInfo = uri.UserInfo.Split(':');
                    var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
                    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                    var host = uri.Host;
                    var port = uri.Port > 0 ? uri.Port : 5432;
                    var database = uri.AbsolutePath.TrimStart('/');

                    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
                }
                catch
                {
                    return connectionString;
                }
            }
            return connectionString;
        }

        /// <summary>
        /// Returns an EmailSettings instance populated with the current Gmail configuration values.
        /// </summary>
        public static EmailSettings ToEmailSettings()
        {
            return new EmailSettings
            {
                SmtpServer = SmtpServer,
                Port = SmtpPort,
                SenderName = SenderName,
                SenderEmail = SenderEmail,
                AppPassword = GmailPassword,
                EnableSsl = EnableSsl
            };
        }
    }

    /// <summary>
    /// Alias for Config class.
    /// </summary>
    public static class AppConfig
    {
        public static string DbConnectionString => Config.DbConnectionString;
        public static string SmtpServer => Config.SmtpServer;
        public static int SmtpPort => Config.SmtpPort;
        public static string SenderName => Config.SenderName;
        public static string SenderEmail => Config.SenderEmail;
        public static string GmailPassword => Config.GmailPassword;
        public static bool EnableSsl => Config.EnableSsl;
        public static string JwtKey => Config.JwtKey;
        public static string JwtIssuer => Config.JwtIssuer;
        public static string JwtAudience => Config.JwtAudience;
        public static int JwtExpiresMinutes => Config.JwtExpiresMinutes;
        public static void Load(IConfiguration configuration) => Config.Load(configuration);
        public static EmailSettings ToEmailSettings() => Config.ToEmailSettings();
    }
}
