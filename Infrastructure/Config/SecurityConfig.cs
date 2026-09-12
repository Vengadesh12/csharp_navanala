namespace MyBackend.Infrastructure.Configuration;

public class SecurityConfig
{
    public int OtpExpiresMinutes { get; set; } = 10;
    public int SessionTimeoutMinutes { get; set; } = 300;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutTimeMinutes { get; set; } = 15;
}
