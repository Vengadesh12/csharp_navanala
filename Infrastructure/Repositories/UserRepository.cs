using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
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
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
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
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
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
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
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
                      OR
                      ({2} > 0 AND p."Id" IN (
                          SELECT up."PermissionId"
                          FROM userpermissions up
                          WHERE up."UserId" = {2}
                      ))
                  )
                """, roleId, designationId, userId).ToListAsync();

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
                      OR
                      ({2} > 0 AND p."Id" IN (
                          SELECT up."PermissionId"
                          FROM userpermissions up
                          WHERE up."UserId" = {2}
                      ))
                  )
                ORDER BY "Value"
                """, roleId, designationId, userId).ToListAsync();
        }

        public async Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE users SET "Password" = {newPasswordHash}, "IsFirstLogin" = false WHERE "Id" = {userId}
                """);
            return rows > 0;
        }

        public async Task<Dictionary<int, string>> GetActiveRolesLookupAsync()
        {
            return await _context.Roles
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "DeletedFlag" = 1
                """)
                .AsNoTracking()
                .ToDictionaryAsync(r => r.Id, r => r.Name);
        }

        public async Task<Dictionary<int, string>> GetActiveDesignationsLookupAsync()
        {
            return await _context.Designations
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DepartmentId", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM designations
                    WHERE "DeletedFlag" = 1
                """)
                .AsNoTracking()
                .ToDictionaryAsync(d => d.Id, d => d.Name);
        }

        public async Task<string?> GetRoleNameByIdAsync(int roleId)
        {
            return await _context.Database.SqlQueryRaw<string>("""
                SELECT "Name" AS "Value"
                FROM roles
                WHERE "Id" = {0} AND "DeletedFlag" = 1
            """, roleId).FirstOrDefaultAsync();
        }

        public async Task<string?> GetDesignationNameByIdAsync(int designationId)
        {
            return await _context.Database.SqlQueryRaw<string>("""
                SELECT "Name" AS "Value"
                FROM designations
                WHERE "Id" = {0} AND "DeletedFlag" = 1
            """, designationId).FirstOrDefaultAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            if (excludeUserId.HasValue)
            {
                var count = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM users
                    WHERE LOWER("Email") = LOWER({0}) AND "DeletedFlag" = 1 AND "Id" <> {1}
                """, email, excludeUserId.Value).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                var count = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM users
                    WHERE LOWER("Email") = LOWER({0}) AND "DeletedFlag" = 1
                """, email).SingleOrDefaultAsync();
                return count > 0;
            }
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null)
        {
            if (excludeUserId.HasValue)
            {
                var count = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM users
                    WHERE "Phone" = {0} AND "DeletedFlag" = 1 AND "Id" <> {1}
                """, phone, excludeUserId.Value).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                var count = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM users
                    WHERE "Phone" = {0} AND "DeletedFlag" = 1
                """, phone).SingleOrDefaultAsync();
                return count > 0;
            }
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1
            """).SingleOrDefaultAsync();
        }

        public async Task<int> GetUsersWithRoleCountAsync()
        {
            return await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1 AND "RoleId" IS NOT NULL
            """).SingleOrDefaultAsync();
        }

        public async Task<List<string>> GetUserPermissionKeysForProfileAsync(int roleId, int designationId)
        {
            if (roleId == 2)
            {
                return await _context.Database.SqlQueryRaw<string>("""
                    SELECT "PermissionKey" AS "Value"
                    FROM permissions
                    WHERE "DeletedFlag" = 1
                    ORDER BY "Id"
                """).ToListAsync();
            }

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

        public async Task<Dictionary<int, string>> GetUserRoleMapAsync()
        {
            try
            {
                var roles = await _context.Roles
                    .FromSqlRaw("""
                        SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                        FROM roles
                        WHERE "DeletedFlag" = 1
                    """)
                    .AsNoTracking()
                    .ToDictionaryAsync(r => r.Id, r => r.Name);

                var userRoles = await _context.Users
                    .FromSqlRaw("""
                        SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                        FROM users
                        WHERE "DeletedFlag" = 1 AND "RoleId" IS NOT NULL
                    """)
                    .AsNoTracking()
                    .Select(u => new { u.Id, RoleId = u.RoleId!.Value })
                    .ToListAsync();

                var map = new Dictionary<int, string>();
                foreach (var ur in userRoles)
                {
                    if (roles.TryGetValue(ur.RoleId, out var roleName))
                    {
                        map[ur.Id] = roleName;
                    }
                }
                return map;
            }
            catch
            {
                return new Dictionary<int, string>();
            }
        }
    }
}
