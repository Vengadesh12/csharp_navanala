using Microsoft.Extensions.Configuration;
using MyBackend.Application.Common.Models;
using System;

namespace MyBackend.Configuration
{
    /// <summary>
    /// Centralized application configuration containing PostgreSQL database connection parameters,
    /// Gmail SMTP credentials, JWT authorization secrets, and numeric security settings.
    /// </summary>
    public static class Config
    {
        // =========================================================================
        // 1. Database Connection Configuration & Integer Parameters
        // =========================================================================
        /// <summary>
        /// PostgreSQL database host address.
        /// </summary>
        public static string DbHost { get; set; } = "localhost";

        /// <summary>
        /// PostgreSQL database port integer (default: 5432).
        /// </summary>
        public static int DbPort { get; set; } = 5432;

        /// <summary>
        /// PostgreSQL database name.
        /// </summary>
        public static string DbName { get; set; } = "postgres";

        /// <summary>
        /// PostgreSQL database username.
        /// </summary>
        public static string DbUser { get; set; } = "postgres";

        /// <summary>
        /// PostgreSQL database password.
        /// </summary>
        public static string DbPassword { get; set; } = "Test";

        /// <summary>
        /// Database command timeout in seconds (integer).
        /// </summary>
        public static int DbTimeout { get; set; } = 30;

        /// <summary>
        /// Maximum connection pool size integer (default: 100).
        /// </summary>
        public static int DbMaxPoolSize { get; set; } = 100;

        /// <summary>
        /// Minimum connection pool size integer (default: 0).
        /// </summary>
        public static int DbMinPoolSize { get; set; } = 0;

        /// <summary>
        /// Connection lifetime in seconds integer (default: 300).
        /// </summary>
        public static int DbConnectionLifeTime { get; set; } = 300;

        private static string? _customDbConnectionString = null;

        /// <summary>
        /// PostgreSQL database connection string. Automatically constructed from connection parameters
        /// or loaded from environment variables / connection string configuration.
        /// </summary>
        public static string DbConnectionString
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_customDbConnectionString))
                {
                    return _customDbConnectionString;
                }
                return $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword};Timeout={DbTimeout};Maximum Pool Size={DbMaxPoolSize};Minimum Pool Size={DbMinPoolSize};Connection Lifetime={DbConnectionLifeTime};";
            }
            set
            {
                _customDbConnectionString = value;
                ParseConnectionStringToProperties(value);
            }
        }

        // =========================================================================
        // 2. Gmail / SMTP Configuration & Integer Parameters
        // =========================================================================
        /// <summary>
        /// SMTP host server address (e.g. smtp.gmail.com).
        /// </summary>
        public static string SmtpServer { get; set; } = "smtp.gmail.com";

        /// <summary>
        /// SMTP port number integer (587 for TLS / STARTTLS, 465 for SSL).
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

        /// <summary>
        /// SMTP client operation timeout in seconds (integer).
        /// </summary>
        public static int SmtpTimeoutSeconds { get; set; } = 30;

        // =========================================================================
        // 3. JWT Security & Session Lifetime Configuration (Integers)
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
        /// JWT Token expiration window in minutes integer (default: 120 minutes).
        /// </summary>
        public static int JwtExpiresMinutes { get; set; } = 120;

        /// <summary>
        /// Refresh token expiration window in days integer (default: 7 days).
        /// </summary>
        public static int RefreshTokenExpiresDays { get; set; } = 7;

        /// <summary>
        /// One-time Password (OTP) verification window in minutes integer (default: 10 minutes).
        /// </summary>
        public static int OtpExpiresMinutes { get; set; } = 10;

        /// <summary>
        /// Active user session inactivity timeout in minutes integer (default: 1440 minutes / 24 hours).
        /// </summary>
        public static int SessionTimeoutMinutes { get; set; } = 1440;

        /// <summary>
        /// Maximum allowed consecutive failed login attempts integer before temporary lockout (default: 5).
        /// </summary>
        public static int MaxFailedAccessAttempts { get; set; } = 5;

        /// <summary>
        /// Lockout duration in minutes integer after exceeding maximum failed login attempts (default: 15 minutes).
        /// </summary>
        public static int LockoutTimeMinutes { get; set; } = 15;

        // =========================================================================
        // Helper Methods & Parsers
        // =========================================================================

        /// <summary>
        /// Synchronizes and overlays configuration values from IConfiguration / environment variables / appsettings.json.
        /// </summary>
        /// <param name="configuration">The application configuration root.</param>
        public static void Load(IConfiguration configuration)
        {
            if (configuration == null) return;

            // Database Connection string and integer parameters
            var connection = configuration.GetConnectionString("DefaultConnection")
                ?? configuration["DATABASE_URL"]
                ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? configuration["ConnectionStrings:DefaultConnection"]
                ?? configuration["DbConnectionString"];

            if (!string.IsNullOrWhiteSpace(connection))
            {
                DbConnectionString = ConvertPostgresUriToNpgsql(connection);
            }

            var dbHost = configuration["Database:Host"] ?? Environment.GetEnvironmentVariable("DB_HOST");
            if (!string.IsNullOrWhiteSpace(dbHost)) DbHost = dbHost;

            var dbPortVal = configuration["Database:Port"] ?? Environment.GetEnvironmentVariable("DB_PORT");
            if (int.TryParse(dbPortVal, out var dbPort) && dbPort > 0)
                DbPort = dbPort;

            var dbName = configuration["Database:Name"] ?? configuration["Database:Database"] ?? Environment.GetEnvironmentVariable("DB_NAME");
            if (!string.IsNullOrWhiteSpace(dbName)) DbName = dbName;

            var dbUser = configuration["Database:User"] ?? configuration["Database:Username"] ?? Environment.GetEnvironmentVariable("DB_USER");
            if (!string.IsNullOrWhiteSpace(dbUser)) DbUser = dbUser;

            var dbPassword = configuration["Database:Password"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD");
            if (!string.IsNullOrWhiteSpace(dbPassword)) DbPassword = dbPassword;

            var dbTimeoutVal = configuration["Database:Timeout"] ?? Environment.GetEnvironmentVariable("DB_TIMEOUT");
            if (int.TryParse(dbTimeoutVal, out var dbTimeout) && dbTimeout > 0)
                DbTimeout = dbTimeout;

            var maxPoolVal = configuration["Database:MaxPoolSize"] ?? Environment.GetEnvironmentVariable("DB_MAX_POOL_SIZE");
            if (int.TryParse(maxPoolVal, out var maxPool) && maxPool > 0)
                DbMaxPoolSize = maxPool;

            var minPoolVal = configuration["Database:MinPoolSize"] ?? Environment.GetEnvironmentVariable("DB_MIN_POOL_SIZE");
            if (int.TryParse(minPoolVal, out var minPool) && minPool >= 0)
                DbMinPoolSize = minPool;

            // Gmail / SMTP Settings
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

            var smtpTimeoutVal = configuration["EmailSettings:TimeoutSeconds"] ?? Environment.GetEnvironmentVariable("SMTP_TIMEOUT_SECONDS");
            if (int.TryParse(smtpTimeoutVal, out var smtpTimeout) && smtpTimeout > 0)
                SmtpTimeoutSeconds = smtpTimeout;

            // JWT Security Settings
            var key = configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
            if (!string.IsNullOrWhiteSpace(key)) JwtKey = key;

            var issuer = configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER");
            if (!string.IsNullOrWhiteSpace(issuer)) JwtIssuer = issuer;

            var audience = configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            if (!string.IsNullOrWhiteSpace(audience)) JwtAudience = audience;

            var expVal = configuration["Jwt:ExpiresMinutes"] ?? Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES");
            if (int.TryParse(expVal, out var exp) && exp > 0)
                JwtExpiresMinutes = exp;

            var refreshExpVal = configuration["Jwt:RefreshTokenExpiresDays"] ?? Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRES_DAYS");
            if (int.TryParse(refreshExpVal, out var refreshExp) && refreshExp > 0)
                RefreshTokenExpiresDays = refreshExp;

            var otpExpVal = configuration["Security:OtpExpiresMinutes"] ?? Environment.GetEnvironmentVariable("OTP_EXPIRES_MINUTES");
            if (int.TryParse(otpExpVal, out var otpExp) && otpExp > 0)
                OtpExpiresMinutes = otpExp;

            var sessionTimeoutVal = configuration["Security:SessionTimeoutMinutes"] ?? Environment.GetEnvironmentVariable("SESSION_TIMEOUT_MINUTES");
            if (int.TryParse(sessionTimeoutVal, out var sessionTimeout) && sessionTimeout > 0)
                SessionTimeoutMinutes = sessionTimeout;

            var maxFailedVal = configuration["Security:MaxFailedAccessAttempts"] ?? Environment.GetEnvironmentVariable("MAX_FAILED_ACCESS_ATTEMPTS");
            if (int.TryParse(maxFailedVal, out var maxFailed) && maxFailed > 0)
                MaxFailedAccessAttempts = maxFailed;

            var lockoutVal = configuration["Security:LockoutTimeMinutes"] ?? Environment.GetEnvironmentVariable("LOCKOUT_TIME_MINUTES");
            if (int.TryParse(lockoutVal, out var lockout) && lockout > 0)
                LockoutTimeMinutes = lockout;
        }

        private static void ParseConnectionStringToProperties(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return;

            try
            {
                var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var kvp = part.Split('=', 2);
                    if (kvp.Length != 2) continue;

                    var key = kvp[0].Trim();
                    var value = kvp[1].Trim();

                    if (key.Equals("Host", StringComparison.OrdinalIgnoreCase) || key.Equals("Server", StringComparison.OrdinalIgnoreCase))
                    {
                        DbHost = value;
                    }
                    else if (key.Equals("Port", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var port))
                    {
                        DbPort = port;
                    }
                    else if (key.Equals("Database", StringComparison.OrdinalIgnoreCase) || key.Equals("Db", StringComparison.OrdinalIgnoreCase))
                    {
                        DbName = value;
                    }
                    else if (key.Equals("Username", StringComparison.OrdinalIgnoreCase) || key.Equals("User Id", StringComparison.OrdinalIgnoreCase) || key.Equals("User", StringComparison.OrdinalIgnoreCase))
                    {
                        DbUser = value;
                    }
                    else if (key.Equals("Password", StringComparison.OrdinalIgnoreCase) || key.Equals("Pwd", StringComparison.OrdinalIgnoreCase))
                    {
                        DbPassword = value;
                    }
                    else if (key.Equals("Timeout", StringComparison.OrdinalIgnoreCase) || key.Equals("CommandTimeout", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out var timeout)) DbTimeout = timeout;
                    }
                    else if (key.Equals("Maximum Pool Size", StringComparison.OrdinalIgnoreCase) || key.Equals("MaxPoolSize", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out var maxPool)) DbMaxPoolSize = maxPool;
                    }
                    else if (key.Equals("Minimum Pool Size", StringComparison.OrdinalIgnoreCase) || key.Equals("MinPoolSize", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out var minPool)) DbMinPoolSize = minPool;
                    }
                }
            }
            catch
            {
                // Fallback gracefully without throwing
            }
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

                    DbHost = host;
                    DbPort = port;
                    DbUser = username;
                    DbPassword = password;
                    DbName = database;

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
    /// Alias for Config class providing backward-compatible and strongly typed access to application configuration properties.
    /// </summary>
    public static class AppConfig
    {
        public static string DbHost => Config.DbHost;
        public static int DbPort => Config.DbPort;
        public static string DbName => Config.DbName;
        public static string DbUser => Config.DbUser;
        public static string DbPassword => Config.DbPassword;
        public static int DbTimeout => Config.DbTimeout;
        public static int DbMaxPoolSize => Config.DbMaxPoolSize;
        public static int DbMinPoolSize => Config.DbMinPoolSize;
        public static int DbConnectionLifeTime => Config.DbConnectionLifeTime;
        public static string DbConnectionString => Config.DbConnectionString;

        public static string SmtpServer => Config.SmtpServer;
        public static int SmtpPort => Config.SmtpPort;
        public static string SenderName => Config.SenderName;
        public static string SenderEmail => Config.SenderEmail;
        public static string GmailPassword => Config.GmailPassword;
        public static bool EnableSsl => Config.EnableSsl;
        public static int SmtpTimeoutSeconds => Config.SmtpTimeoutSeconds;

        public static string JwtKey => Config.JwtKey;
        public static string JwtIssuer => Config.JwtIssuer;
        public static string JwtAudience => Config.JwtAudience;
        public static int JwtExpiresMinutes => Config.JwtExpiresMinutes;
        public static int RefreshTokenExpiresDays => Config.RefreshTokenExpiresDays;
        public static int OtpExpiresMinutes => Config.OtpExpiresMinutes;
        public static int SessionTimeoutMinutes => Config.SessionTimeoutMinutes;
        public static int MaxFailedAccessAttempts => Config.MaxFailedAccessAttempts;
        public static int LockoutTimeMinutes => Config.LockoutTimeMinutes;

        public static void Load(IConfiguration configuration) => Config.Load(configuration);
        public static EmailSettings ToEmailSettings() => Config.ToEmailSettings();
    }

}
