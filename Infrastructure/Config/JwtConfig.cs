namespace MyBackend.Infrastructure.Configuration;

public class JwtConfig
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Userspace";
    public string Audience { get; set; } = "Userspace.Web";
    public int ExpiresMinutes { get; set; } = 525600;
    public int RefreshTokenExpiresDays { get; set; } = 7;
}
