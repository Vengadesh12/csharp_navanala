using System.Collections.Generic;
using System.Security.Claims;

namespace MyBackend.Application.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Email { get; }
        string? Name { get; }
        string? Role { get; }
        int? RoleId { get; }
        int? DesignationId { get; }
        bool IsAuthenticated { get; }
        ClaimsPrincipal? User { get; }
        IEnumerable<Claim> Claims { get; }
        string? GetClaimValue(string claimType);
    }
}
