using System.Collections.Generic;
using System.Security.Claims;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(UserModel user, string? roleName = null, IEnumerable<string>? permissions = null, int? sessionId = null);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
        (string? Email, string? Name, string? Picture) ReadTokenPayload(string idToken);
    }
}
