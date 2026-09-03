using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyBackend.Application.Interfaces;
using MyBackend.Configuration;
using MyBackend.Domain.Entities;

namespace MyBackend.Infrastructure.Services
{
    /// <summary>
    /// Implements JWT token generation, reading, and validation using configured signing credentials.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user, string? roleName = null, IEnumerable<string>? permissions = null, int? sessionId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleId?.ToString() ?? string.Empty)
            };

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                claims.Add(new Claim("role_name", roleName));
            }

            if (sessionId.HasValue)
            {
                claims.Add(new Claim("session_id", sessionId.Value.ToString()));
            }

            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }
            }

            var jwtKey = !string.IsNullOrWhiteSpace(_configuration["Jwt:Key"]) ? _configuration["Jwt:Key"]! : Config.JwtKey;
            var jwtIssuer = !string.IsNullOrWhiteSpace(_configuration["Jwt:Issuer"]) ? _configuration["Jwt:Issuer"]! : Config.JwtIssuer;
            var jwtAudience = !string.IsNullOrWhiteSpace(_configuration["Jwt:Audience"]) ? _configuration["Jwt:Audience"]! : Config.JwtAudience;
            var jwtExpires = _configuration.GetValue<int>("Jwt:ExpiresMinutes", Config.JwtExpiresMinutes);

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpires),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            try
            {
                var jwtKey = !string.IsNullOrWhiteSpace(_configuration["Jwt:Key"]) ? _configuration["Jwt:Key"]! : Config.JwtKey;
                var jwtIssuer = !string.IsNullOrWhiteSpace(_configuration["Jwt:Issuer"]) ? _configuration["Jwt:Issuer"]! : Config.JwtIssuer;
                var jwtAudience = !string.IsNullOrWhiteSpace(_configuration["Jwt:Audience"]) ? _configuration["Jwt:Audience"]! : Config.JwtAudience;

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public (string? Email, string? Name, string? Picture) ReadTokenPayload(string idToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(idToken)) return (null, null, null);

                var jwt = handler.ReadJwtToken(idToken);
                var tokenEmail = jwt.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value;
                var tokenName = jwt.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == ClaimTypes.Name)?.Value;
                var tokenPicture = jwt.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

                return (tokenEmail, tokenName, tokenPicture);
            }
            catch
            {
                return (null, null, null);
            }
        }
    }
}
