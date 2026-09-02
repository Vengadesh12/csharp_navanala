using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using MyBackend.Application.Common.Models;
using MyBackend.Domain.Entities;
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

    // =========================================================================
    // 4. Fluent API Entity Type Configurations (IEntityTypeConfiguration<T>)
    // =========================================================================

    /// <summary>
    /// Fluent API Entity Type Configuration for User entity.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("Email")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .HasColumnName("Password")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.RoleId)
                .HasColumnName("RoleId");

            builder.Property(x => x.DesignationId)
                .HasColumnName("DesignationId");

            builder.Property(x => x.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(50);

            builder.Property(x => x.Age)
                .HasColumnName("Age");

            builder.Property(x => x.Address)
                .HasColumnName("Address")
                .HasMaxLength(500);

            builder.Property(x => x.ProfileImage)
                .HasColumnName("ProfileImage")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.IsFirstLogin)
                .HasColumnName("IsFirstLogin")
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.Ignore(x => x.Password);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Role entity.
    /// </summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Department entity.
    /// </summary>
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("departments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Designation entity.
    /// </summary>
    public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
    {
        public void Configure(EntityTypeBuilder<Designation> builder)
        {
            builder.ToTable("designations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(x => x.DepartmentId)
                .HasColumnName("DepartmentId");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.HasOne(x => x.Department)
                .WithMany(d => d.Designations)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Permission entity.
    /// </summary>
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("permissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.PermissionKey)
                .HasColumnName("PermissionKey")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for RolePermission entity.
    /// </summary>
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("rolepermissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.RoleId)
                .HasColumnName("RoleId")
                .IsRequired();

            builder.Property(x => x.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for DepartmentPermission entity.
    /// </summary>
    public class DepartmentPermissionConfiguration : IEntityTypeConfiguration<DepartmentPermission>
    {
        public void Configure(EntityTypeBuilder<DepartmentPermission> builder)
        {
            builder.ToTable("departmentpermissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.DepartmentId)
                .HasColumnName("DepartmentId")
                .IsRequired();

            builder.Property(x => x.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for UserSession entity.
    /// </summary>
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.ToTable("user_sessions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(150);

            builder.Property(x => x.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(100);

            builder.Property(x => x.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(50);

            builder.Property(x => x.UserAgent)
                .HasColumnName("user_agent")
                .HasMaxLength(500);

            builder.Property(x => x.LoginTime)
                .HasColumnName("login_time")
                .IsRequired();

            builder.Property(x => x.LogoutTime)
                .HasColumnName("logout_time");

            builder.Property(x => x.SessionToken)
                .HasColumnName("session_token")
                .HasMaxLength(255);

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Menu entity.
    /// </summary>
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable("menus");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.MenuKey)
                .HasColumnName("menukey")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Label)
                .HasColumnName("label")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Icon)
                .HasColumnName("icon")
                .HasMaxLength(50);

            builder.Property(x => x.Route)
                .HasColumnName("route")
                .HasMaxLength(200);

            builder.Property(x => x.GroupName)
                .HasColumnName("groupname")
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.OrderIndex)
                .HasColumnName("orderindex")
                .HasDefaultValue(0);

            builder.Property(x => x.PermissionKey)
                .HasColumnName("permissionkey")
                .HasMaxLength(100);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deletedflag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for AuditLog entity.
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("audit_logs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Action)
                .HasColumnName("action")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Module)
                .HasColumnName("module")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.PerformedBy)
                .HasColumnName("performed_by")
                .HasMaxLength(150);

            builder.Property(x => x.Details)
                .HasColumnName("details");

            builder.Property(x => x.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Report entity.
    /// </summary>
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("reports");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.CategoryId)
                .HasColumnName("category_id");

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Format)
                .HasColumnName("format")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(150);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.FileSize)
                .HasColumnName("file_size")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for ReportCategory entity.
    /// </summary>
    public class ReportCategoryConfiguration : IEntityTypeConfiguration<ReportCategory>
    {
        public void Configure(EntityTypeBuilder<ReportCategory> builder)
        {
            builder.ToTable("report_categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Project entity.
    /// </summary>
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("projects");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(x => x.LeadName)
                .HasColumnName("lead_name")
                .HasMaxLength(150);

            builder.Property(x => x.ProgressPercentage)
                .HasColumnName("progress_percentage");

            builder.Property(x => x.DueDate)
                .HasColumnName("due_date");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for ProjectCategory entity.
    /// </summary>
    public class ProjectCategoryConfiguration : IEntityTypeConfiguration<ProjectCategory>
    {
        public void Configure(EntityTypeBuilder<ProjectCategory> builder)
        {
            builder.ToTable("project_categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for ScheduleEvent entity.
    /// </summary>
    public class ScheduleEventConfiguration : IEntityTypeConfiguration<ScheduleEvent>
    {
        public void Configure(EntityTypeBuilder<ScheduleEvent> builder)
        {
            builder.ToTable("schedules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(50);

            builder.Property(x => x.EventDate)
                .HasColumnName("event_date");

            builder.Property(x => x.StartTime)
                .HasColumnName("start_time")
                .HasMaxLength(50);

            builder.Property(x => x.EndTime)
                .HasColumnName("end_time")
                .HasMaxLength(50);

            builder.Property(x => x.Location)
                .HasColumnName("location")
                .HasMaxLength(200);

            builder.Property(x => x.Organizer)
                .HasColumnName("organizer")
                .HasMaxLength(150);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(x => x.AttendeesCount)
                .HasColumnName("attendees_count");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for SystemSetting entity.
    /// </summary>
    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.ToTable("system_settings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.SettingKey)
                .HasColumnName("setting_key")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.SettingValue)
                .HasColumnName("setting_value");

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.DataType)
                .HasColumnName("data_type")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(150);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for SettingCategory entity.
    /// </summary>
    public class SettingCategoryConfiguration : IEntityTypeConfiguration<SettingCategory>
    {
        public void Configure(EntityTypeBuilder<SettingCategory> builder)
        {
            builder.ToTable("setting_categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.Icon)
                .HasColumnName("icon")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(150);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for EventType entity.
    /// </summary>
    public class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
    {
        public void Configure(EntityTypeBuilder<EventType> builder)
        {
            builder.ToTable("event_types");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.Color)
                .HasColumnName("color")
                .HasMaxLength(50);

            builder.Property(x => x.Icon)
                .HasColumnName("icon")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(150);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for ApprovalRequest entity.
    /// </summary>
    public class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
    {
        public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
        {
            builder.ToTable("approval_requests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.EmployeeName)
                .HasColumnName("employee_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.EmployeeEmail)
                .HasColumnName("employee_email")
                .HasMaxLength(150);

            builder.Property(x => x.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(100);

            builder.Property(x => x.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(x => x.EstimatedAmount)
                .HasColumnName("estimated_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.Comments)
                .HasColumnName("comments")
                .HasMaxLength(1000);

            builder.Property(x => x.ReviewedById)
                .HasColumnName("reviewed_by_id");

            builder.Property(x => x.ReviewedByName)
                .HasColumnName("reviewed_by_name")
                .HasMaxLength(150);

            builder.Property(x => x.ReviewedAt)
                .HasColumnName("reviewed_at");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Purchase entity.
    /// </summary>
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.ToTable("purchases");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.ApprovalRequestId)
                .HasColumnName("approval_request_id");

            builder.Property(x => x.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.EstimatedAmount)
                .HasColumnName("estimated_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.EmployeeName)
                .HasColumnName("employee_name")
                .HasMaxLength(150);

            builder.Property(x => x.EmployeeEmail)
                .HasColumnName("employee_email")
                .HasMaxLength(150);

            builder.Property(x => x.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(100);

            builder.Property(x => x.VendorName)
                .HasColumnName("vendor_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.VendorContact)
                .HasColumnName("vendor_contact")
                .HasMaxLength(100);

            builder.Property(x => x.VendorEmail)
                .HasColumnName("vendor_email")
                .HasMaxLength(150);

            builder.Property(x => x.QuotationNumber)
                .HasColumnName("quotation_number")
                .HasMaxLength(100);

            builder.Property(x => x.QuotationAmount)
                .HasColumnName("quotation_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.QuotationDate)
                .HasColumnName("quotation_date");

            builder.Property(x => x.DeliveryTimeline)
                .HasColumnName("delivery_timeline")
                .HasMaxLength(100);

            builder.Property(x => x.PaymentTerms)
                .HasColumnName("payment_terms")
                .HasMaxLength(200);

            builder.Property(x => x.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            builder.Property(x => x.CreatedByName)
                .HasColumnName("created_by_name")
                .HasMaxLength(150);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for Invoice entity.
    /// </summary>
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("invoices");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.InvoiceNumber)
                .HasColumnName("invoice_number")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CustomerName)
                .HasColumnName("customer_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.CustomerEmail)
                .HasColumnName("customer_email")
                .HasMaxLength(150);

            builder.Property(x => x.CustomerPhone)
                .HasColumnName("customer_phone")
                .HasMaxLength(50);

            builder.Property(x => x.CustomerAddress)
                .HasColumnName("customer_address");

            builder.Property(x => x.CustomerGstin)
                .HasColumnName("customer_gstin")
                .HasMaxLength(50);

            builder.Property(x => x.CompanyGstin)
                .HasColumnName("company_gstin")
                .HasMaxLength(50);

            builder.Property(x => x.InvoiceDate)
                .HasColumnName("invoice_date")
                .IsRequired();

            builder.Property(x => x.DueDate)
                .HasColumnName("due_date");

            builder.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasPrecision(18, 2);

            builder.Property(x => x.TaxRate)
                .HasColumnName("tax_rate")
                .HasPrecision(5, 2);

            builder.Property(x => x.TaxAmount)
                .HasColumnName("tax_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountAmount)
                .HasColumnName("discount_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmountInWords)
                .HasColumnName("total_amount_in_words")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.PaymentMethod)
                .HasColumnName("payment_method")
                .HasMaxLength(50);

            builder.Property(x => x.Notes)
                .HasColumnName("notes");

            builder.Property(x => x.TermsAndConditions)
                .HasColumnName("terms_and_conditions");

            builder.Property(x => x.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            builder.Property(x => x.CreatedByName)
                .HasColumnName("created_by_name")
                .HasMaxLength(150);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.HasMany(x => x.Items)
                .WithOne(i => i.Invoice)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Fluent API Entity Type Configuration for InvoiceItem entity.
    /// </summary>
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.ToTable("invoice_items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.InvoiceId)
                .HasColumnName("invoice_id")
                .IsRequired();

            builder.Property(x => x.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description");

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasPrecision(18, 2);

            builder.Property(x => x.TaxRate)
                .HasColumnName("tax_rate")
                .HasPrecision(5, 2);

            builder.Property(x => x.TaxAmount)
                .HasColumnName("tax_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.OrderIndex)
                .HasColumnName("order_index")
                .HasDefaultValue(0);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
    {
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            builder.ToTable("userpermissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.UserId)
                .HasColumnName("UserId")
                .IsRequired();

            builder.Property(x => x.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    public class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
    {
        public void Configure(EntityTypeBuilder<AccessRequest> builder)
        {
            builder.ToTable("access_requests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.UserEmail)
                .HasColumnName("user_email")
                .HasMaxLength(150);

            builder.Property(x => x.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(150);

            builder.Property(x => x.RoleName)
                .HasColumnName("role_name")
                .HasMaxLength(150);

            builder.Property(x => x.PermissionKey)
                .HasColumnName("permission_key")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.PermissionName)
                .HasColumnName("permission_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Module)
                .HasColumnName("module")
                .HasMaxLength(100);

            builder.Property(x => x.Reason)
                .HasColumnName("reason")
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.ReviewerId)
                .HasColumnName("reviewer_id");

            builder.Property(x => x.ReviewerName)
                .HasColumnName("reviewer_name")
                .HasMaxLength(150);

            builder.Property(x => x.ReviewerComments)
                .HasColumnName("reviewer_comments");

            builder.Property(x => x.ReviewedAt)
                .HasColumnName("reviewed_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }
}