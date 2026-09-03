using System.Collections.Generic;
using System.Security.Claims;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for generating and validating JSON Web Tokens (JWT).
    /// </summary>
    public interface IJwtService
    {
        string GenerateToken(User user, string? roleName = null, IEnumerable<string>? permissions = null, int? sessionId = null);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
        (string? Email, string? Name, string? Picture) ReadTokenPayload(string idToken);
    }
}
