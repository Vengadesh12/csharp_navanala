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
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", COALESCE("DeletedFlag", 1) AS "DeletedFlag"
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
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", COALESCE("DeletedFlag", 1) AS "DeletedFlag"
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
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", COALESCE("DeletedFlag", 1) AS "DeletedFlag"
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

            var userRoleId = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.DeletedFlag == 1)
                .Select(u => u.RoleId)
                .FirstOrDefaultAsync();

            if (!userRoleId.HasValue) return false;

            // Super Admin role ID = 2 has all permissions
            if (userRoleId.Value == 2) return true;

            var permissions = await _context.Database.SqlQueryRaw<string>("""
                SELECT p."PermissionKey" AS "Value"
                FROM permissions p
                INNER JOIN rolepermissions rp ON rp."PermissionId" = p."Id"
                WHERE rp."RoleId" = {0} AND p."DeletedFlag" = 1
                """, userRoleId.Value).ToListAsync();

            return permissionKeys.Any(k => permissions.Contains(k, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<List<string>> GetUserPermissionKeysAsync(int userId)
        {
            var userRoleId = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.DeletedFlag == 1)
                .Select(u => u.RoleId)
                .FirstOrDefaultAsync();

            if (!userRoleId.HasValue) return [];

            // Super Admin role ID = 2 has all permissions
            if (userRoleId.Value == 2)
            {
                return await _context.Permissions
                    .AsNoTracking()
                    .Where(p => p.DeletedFlag == 1)
                    .OrderBy(p => p.Id)
                    .Select(p => p.PermissionKey)
                    .ToListAsync();
            }

            return await _context.Database.SqlQueryRaw<string>("""
                SELECT p."PermissionKey" AS "Value"
                FROM permissions p
                INNER JOIN rolepermissions rp ON rp."PermissionId" = p."Id"
                WHERE rp."RoleId" = {0} AND p."DeletedFlag" = 1
                ORDER BY p."Id"
                """, userRoleId.Value).ToListAsync();
        }

        public async Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE users SET "Password" = {newPasswordHash} WHERE "Id" = {userId}
                """);
            return rows > 0;
        }
    }
}
