using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.Interfaces;
using MyBackend.Application.Common.Validators;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public ProfileService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileResponse> GetProfileAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedFlag == 1);

            if (user == null)
            {
                throw new NotFoundException("User profile not found or account is deactivated.");
            }

            string roleName = "Member";
            if (user.RoleId.HasValue)
            {
                roleName = await _context.Roles
                    .Where(r => r.Id == user.RoleId && r.DeletedFlag == 1)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync() ?? "Member";
            }

            var roleId = user.RoleId ?? 0;
            var designationId = user.DesignationId ?? 0;

            List<string> permissions;
            if (roleId == 2)
            {
                permissions = await _context.Permissions
                    .AsNoTracking()
                    .Where(p => p.DeletedFlag == 1)
                    .OrderBy(p => p.Id)
                    .Select(p => p.PermissionKey)
                    .ToListAsync();
            }
            else
            {
                permissions = await _context.Database.SqlQueryRaw<string>("""
                    SELECT DISTINCT p."PermissionKey" AS "Value"
                    FROM permissions p
                    WHERE p."DeletedFlag" = 1
                      AND (
                          ({0} > 0 AND p."Id" IN (
                              SELECT rp."PermissionId" 
                              FROM rolepermissions rp 
                              WHERE rp."RoleId" = {0}
                          ))
                          OR
                          ({1} > 0 AND p."Id" IN (
                              SELECT dp."PermissionId"
                              FROM departmentpermissions dp
                              INNER JOIN designations des ON des."DepartmentId" = dp."DepartmentId" AND des."DeletedFlag" = 1
                              WHERE des."Id" = {1}
                          ))
                      )
                    ORDER BY "Value"
                    """, roleId, designationId).ToListAsync();
            }

            return new UserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Age = user.Age,
                Address = user.Address,
                RoleId = user.RoleId,
                RoleName = roleName,
                Permissions = permissions,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedFlag == 1);
            if (user == null)
            {
                throw new NotFoundException("User profile not found.");
            }

            user.UpdateDetails(
                name: !string.IsNullOrWhiteSpace(request.Name) ? request.Name : user.Name,
                email: !string.IsNullOrWhiteSpace(request.Email) ? request.Email : user.Email,
                phone: request.Phone ?? user.Phone,
                age: request.Age > 0 ? request.Age : user.Age,
                address: request.Address ?? user.Address,
                roleId: user.RoleId,
                designationId: user.DesignationId
            );

            await _context.SaveChangesAsync();

            return new UserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Age = user.Age,
                Address = user.Address,
                RoleId = user.RoleId,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                throw new BadRequestException("Current password is required.");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new BadRequestException("New password is required.");
            }

            var (isValid, errors) = PasswordValidator.Validate(request.NewPassword);
            if (!isValid)
            {
                throw new BadRequestException(errors.Count > 0 ? errors[0] : "New password does not meet strong security requirements.");
            }

            if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            {
                throw new BadRequestException("New password and confirm password do not match.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedFlag == 1);
            if (user == null)
            {
                throw new NotFoundException("User profile not found.");
            }

            // Verify current password if user has one
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
                if (verify == PasswordVerificationResult.Failed && !string.Equals(user.PasswordHash, request.CurrentPassword, StringComparison.Ordinal))
                {
                    throw new BadRequestException("Current password entered is incorrect.");
                }
            }

            var newHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.SetPasswordHash(newHash);
            user.CompleteFirstLogin();

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
