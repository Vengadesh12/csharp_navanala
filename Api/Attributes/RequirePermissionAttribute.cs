using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string[] PermissionKeys { get; }
        public string? Menu { get; }
        public string? Action { get; }

        public RequirePermissionAttribute(params string[] permissionKeys)
        {
            PermissionKeys = permissionKeys ?? [];
        }

        public RequirePermissionAttribute(string menu, string action)
        {
            Menu = menu;
            Action = action;
            PermissionKeys = new[] { $"{menu}.{action}" };
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponse
                {
                    Success = false,
                    Message = "Authentication required. Please provide a valid token."
                });
                return;
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("userId")?.Value;

            if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponse
                {
                    Success = false,
                    Message = "A valid user session is required."
                });
                return;
            }

            var rbacService = context.HttpContext.RequestServices.GetRequiredService<IPermissionHierarchyService>();

            bool isAuthorized = false;

            if (!string.IsNullOrWhiteSpace(Menu) && !string.IsNullOrWhiteSpace(Action))
            {
                isAuthorized = await rbacService.HasActionAccessAsync(userId, Menu, Action);
            }
            else if (PermissionKeys.Length > 0)
            {
                foreach (var key in PermissionKeys)
                {
                    if (await rbacService.HasPermissionAsync(userId, key))
                    {
                        isAuthorized = true;
                        break;
                    }
                }
            }
            else
            {
                isAuthorized = true;
            }

            if (!isAuthorized)
            {
                context.Result = new ObjectResult(new ErrorResponse
                {
                    Success = false,
                    Message = "Access Denied. You do not possess the required permission to perform this action.",
                    Data = null
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string[] Roles { get; }

        public RequireRoleAttribute(params string[] roles)
        {
            Roles = roles ?? [];
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponse
                {
                    Success = false,
                    Message = "Authentication required. Please provide a valid token."
                });
                return Task.CompletedTask;
            }

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value;
            var roleIdClaim = user.FindFirst("roleId")?.Value ?? user.FindFirst("RoleId")?.Value;
            int.TryParse(roleIdClaim, out var roleId);

            // Super Admin (roleId 2 or Super Admin role name) always passes
            if (roleId == 2 || string.Equals(userRole, "Super Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var hasRole = Roles.Any(r =>
                string.Equals(r, userRole, StringComparison.OrdinalIgnoreCase) ||
                (int.TryParse(r, out var rId) && rId == roleId));

            if (!hasRole)
            {
                context.Result = new ObjectResult(new ErrorResponse
                {
                    Success = false,
                    Message = $"Access Denied. This endpoint requires one of the following roles: {string.Join(", ", Roles)}.",
                    Data = null
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            return Task.CompletedTask;
        }
    }
}
