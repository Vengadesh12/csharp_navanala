using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyBackend.Application.Interfaces;

namespace MyBackend.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public int? UserId
        {
            get
            {
                var idStr = GetClaimValue(ClaimTypes.NameIdentifier)
                    ?? GetClaimValue("sub")
                    ?? GetClaimValue("userId")
                    ?? GetClaimValue("id");

                return int.TryParse(idStr, out var id) ? id : null;
            }
        }

        public string? Email =>
            GetClaimValue(ClaimTypes.Email) ?? GetClaimValue("email");

        public string? Name =>
            GetClaimValue(ClaimTypes.Name) ?? GetClaimValue("name");

        public string? Role =>
            GetClaimValue(ClaimTypes.Role) ?? GetClaimValue("role");

        public int? RoleId
        {
            get
            {
                var roleIdStr = GetClaimValue("roleId") ?? GetClaimValue("RoleId");
                return int.TryParse(roleIdStr, out var roleId) ? roleId : null;
            }
        }

        public int? DesignationId
        {
            get
            {
                var desIdStr = GetClaimValue("designationId") ?? GetClaimValue("DesignationId");
                return int.TryParse(desIdStr, out var desId) ? desId : null;
            }
        }

        public IEnumerable<Claim> Claims =>
            User?.Claims ?? Enumerable.Empty<Claim>();

        public string? GetClaimValue(string claimType)
        {
            return User?.FindFirst(c => c.Type.Equals(claimType, System.StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }
}
