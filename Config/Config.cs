using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;

namespace MyBackend.Configuration
{
    /// <summary>
    /// Email communication settings model.
    /// </summary>
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string AppPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Centralized application configuration containing PostgreSQL database connection parameters,
    /// Gmail SMTP credentials, JWT authorization secrets, and numeric security settings.
    /// Secrets are loaded dynamically from environment variables, config.json, or appsettings.json.
    /// </summary>
    public static class Config
    {
        // =========================================================================
        // 1. Database Connection Configuration & Integer Parameters
        // =========================================================================
        public static string DbHost { get; set; } = "localhost";
        public static int DbPort { get; set; } = 5432;
        public static string DbName { get; set; } = "postgres";
        public static string DbUser { get; set; } = "postgres";
        public static string DbPassword { get; set; } = string.Empty;
        public static int DbTimeout { get; set; } = 30;
        public static int DbMaxPoolSize { get; set; } = 100;
        public static int DbMinPoolSize { get; set; } = 0;
        public static int DbConnectionLifeTime { get; set; } = 300;

        private static string? _customDbConnectionString = null;

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
        public static string SmtpServer { get; set; } = "smtp.gmail.com";
        public static int SmtpPort { get; set; } = 587;
        public static string SenderName { get; set; } = "Workspace Administration";
        public static string SenderEmail { get; set; } = string.Empty;
        public static string GmailPassword { get; set; } = string.Empty;
        public static bool EnableSsl { get; set; } = true;
        public static int SmtpTimeoutSeconds { get; set; } = 30;

        // =========================================================================
        // 3. JWT Security & Session Lifetime Configuration (Integers)
        // =========================================================================
        public static string JwtKey { get; set; } = string.Empty;
        public static string JwtIssuer { get; set; } = "Userspace";
        public static string JwtAudience { get; set; } = "Userspace.Web";
        public static int JwtExpiresMinutes { get; set; } = 120;
        public static int RefreshTokenExpiresDays { get; set; } = 7;
        public static int OtpExpiresMinutes { get; set; } = 10;
        public static int SessionTimeoutMinutes { get; set; } = 1440;
        public static int MaxFailedAccessAttempts { get; set; } = 5;
        public static int LockoutTimeMinutes { get; set; } = 15;

        // =========================================================================
        // Helper Methods & Parsers
        // =========================================================================

        /// <summary>
        /// Synchronizes and overlays configuration values from IConfiguration / environment variables / config.json / appsettings.json.
        /// </summary>
        public static void Load(IConfiguration configuration)
        {
            // First load from config.json if available
            TryLoadFromFile();

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

        private static void TryLoadFromFile()
        {
            try
            {
                var candidatePaths = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "Config", "config.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "config.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Config", "config.json")
                };

                foreach (var path in candidatePaths)
                {
                    if (File.Exists(path))
                    {
                        var json = File.ReadAllText(path);
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("Database", out var db))
                        {
                            if (db.TryGetProperty("Host", out var p)) DbHost = p.GetString() ?? DbHost;
                            if (db.TryGetProperty("Port", out var pPort) && pPort.TryGetInt32(out var pt)) DbPort = pt;
                            if (db.TryGetProperty("Name", out var pName)) DbName = pName.GetString() ?? DbName;
                            if (db.TryGetProperty("Username", out var pUser)) DbUser = pUser.GetString() ?? DbUser;
                            if (db.TryGetProperty("Password", out var pPwd)) DbPassword = pPwd.GetString() ?? DbPassword;
                            if (db.TryGetProperty("Timeout", out var pTimeout) && pTimeout.TryGetInt32(out var to)) DbTimeout = to;
                            if (db.TryGetProperty("MaxPoolSize", out var pMax) && pMax.TryGetInt32(out var max)) DbMaxPoolSize = max;
                            if (db.TryGetProperty("MinPoolSize", out var pMin) && pMin.TryGetInt32(out var min)) DbMinPoolSize = min;
                            if (db.TryGetProperty("ConnectionLifetime", out var pLt) && pLt.TryGetInt32(out var lt)) DbConnectionLifeTime = lt;
                        }

                        if (root.TryGetProperty("EmailSettings", out var email))
                        {
                            if (email.TryGetProperty("SmtpServer", out var s)) SmtpServer = s.GetString() ?? SmtpServer;
                            if (email.TryGetProperty("Port", out var ep) && ep.TryGetInt32(out var ePort)) SmtpPort = ePort;
                            if (email.TryGetProperty("SenderName", out var sn)) SenderName = sn.GetString() ?? SenderName;
                            if (email.TryGetProperty("SenderEmail", out var se)) SenderEmail = se.GetString() ?? SenderEmail;
                            if (email.TryGetProperty("AppPassword", out var ap)) GmailPassword = ap.GetString() ?? GmailPassword;
                            if (email.TryGetProperty("EnableSsl", out var ssl)) EnableSsl = ssl.GetBoolean();
                            if (email.TryGetProperty("TimeoutSeconds", out var ts) && ts.TryGetInt32(out var sec)) SmtpTimeoutSeconds = sec;
                        }

                        if (root.TryGetProperty("Jwt", out var jwt))
                        {
                            if (jwt.TryGetProperty("Key", out var k)) JwtKey = k.GetString() ?? JwtKey;
                            if (jwt.TryGetProperty("Issuer", out var iss)) JwtIssuer = iss.GetString() ?? JwtIssuer;
                            if (jwt.TryGetProperty("Audience", out var aud)) JwtAudience = aud.GetString() ?? JwtAudience;
                            if (jwt.TryGetProperty("ExpiresMinutes", out var em) && em.TryGetInt32(out var min)) JwtExpiresMinutes = min;
                            if (jwt.TryGetProperty("RefreshTokenExpiresDays", out var red) && red.TryGetInt32(out var days)) RefreshTokenExpiresDays = days;
                        }

                        if (root.TryGetProperty("Security", out var secObj))
                        {
                            if (secObj.TryGetProperty("OtpExpiresMinutes", out var o) && o.TryGetInt32(out var otp)) OtpExpiresMinutes = otp;
                            if (secObj.TryGetProperty("SessionTimeoutMinutes", out var st) && st.TryGetInt32(out var stm)) SessionTimeoutMinutes = stm;
                            if (secObj.TryGetProperty("MaxFailedAccessAttempts", out var mf) && mf.TryGetInt32(out var mfa)) MaxFailedAccessAttempts = mfa;
                            if (secObj.TryGetProperty("LockoutTimeMinutes", out var lt) && lt.TryGetInt32(out var ltm)) LockoutTimeMinutes = ltm;
                        }

                        break;
                    }
                }
            }
            catch
            {
                // Fallback gracefully without crashing
            }
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
                // Fallback gracefully
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

        public static EmailSettings ToEmailSettings()
        {
            return new EmailSettings
            {
                SmtpServer = SmtpServer,
                Port = SmtpPort,
                SenderName = SenderName,
                SenderEmail = SenderEmail,
                AppPassword = GmailPassword,
                EnableSsl = EnableSsl,
                TimeoutSeconds = SmtpTimeoutSeconds
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
