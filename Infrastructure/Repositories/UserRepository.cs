using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    /// <summary>
    /// Implements specialized User entity queries, raw SQL queries, and RBAC permission checks.
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await _context.Users
                .FromSqlInterpolated($"""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin"
                    FROM users
                    WHERE LOWER("Email") = {normalizedEmail}
                    LIMIT 1
                    """)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin"
                    FROM users
                    ORDER BY "Id"
                    """)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FromSqlInterpolated($"""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin"
                    FROM users
                    WHERE "Id" = {id}
                    """)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<bool> SetDeletedFlagAsync(int id, int deletedFlag)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE users SET "DeletedFlag" = {deletedFlag} WHERE "Id" = {id}
                """);
            return rows > 0;
        }

        public async Task<bool> HasPermissionAsync(int userId, params string[] permissionKeys)
        {
            if (permissionKeys.Length == 0) return true;

            var userRecord = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.DeletedFlag == 1)
                .Select(u => new { u.RoleId, u.DesignationId })
                .FirstOrDefaultAsync();

            if (userRecord is null) return false;

            // Super Admin role ID = 2 has all permissions
            if (userRecord.RoleId == 2) return true;

            var roleId = userRecord.RoleId ?? 0;
            var designationId = userRecord.DesignationId ?? 0;

            var permissions = await _context.Database.SqlQueryRaw<string>("""
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
                """, roleId, designationId).ToListAsync();

            return permissionKeys.Any(k => permissions.Contains(k, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<List<string>> GetUserPermissionKeysAsync(int userId)
        {
            var userRecord = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.DeletedFlag == 1)
                .Select(u => new { u.RoleId, u.DesignationId })
                .FirstOrDefaultAsync();

            if (userRecord is null) return [];

            // Super Admin role ID = 2 has all active permissions
            if (userRecord.RoleId == 2)
            {
                return await _context.Permissions
                    .AsNoTracking()
                    .Where(p => p.DeletedFlag == 1)
                    .OrderBy(p => p.Id)
                    .Select(p => p.PermissionKey)
                    .ToListAsync();
            }

            var roleId = userRecord.RoleId ?? 0;
            var designationId = userRecord.DesignationId ?? 0;

            return await _context.Database.SqlQueryRaw<string>("""
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

        public async Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE users SET "Password" = {newPasswordHash}, "IsFirstLogin" = false WHERE "Id" = {userId}
                """);
            return rows > 0;
        }
    }
}
