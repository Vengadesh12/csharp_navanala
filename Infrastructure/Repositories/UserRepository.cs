using System.Text;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class UserRepository : Repository<UserModel>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<UserModel?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var sql = new StringBuilder("""
                SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                FROM users
                WHERE LOWER("Email") = {0}
                LIMIT 1
                """);

            return await _context.Users
                .FromSqlRaw(sql.ToString(), normalizedEmail)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<UserLoginDetailsModel?> GetLoginUserDetailsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var sql = new StringBuilder("""
                SELECT 
                    u."Id",
                    u."Name",
                    u."Email",
                    u."Password" AS "PasswordHash",
                    u."RoleId",
                    r."Name" AS "RoleName",
                    u."DesignationId",
                    des."Name" AS "DesignationName",
                    dept."Name" AS "DepartmentName",
                    COALESCE(u."Phone", '') AS "Phone",
                    COALESCE(u."Age", 0) AS "Age",
                    COALESCE(u."Address", '') AS "Address",
                    u."ProfileImage",
                    COALESCE(u."DeletedFlag", 1) AS "DeletedFlag",
                    COALESCE(u."IsFirstLogin", false) AS "IsFirstLogin",
                    u."CreatedAt",
                    u."UpdatedAt",
                    COALESCE((
                        SELECT STRING_AGG(DISTINCT p."PermissionKey", ',')
                        FROM permissions p
                        WHERE p."DeletedFlag" = 1
                          AND (
                              u."RoleId" = 2
                              OR (u."RoleId" IS NOT NULL AND p."Id" IN (SELECT rp."PermissionId" FROM rolepermissions rp WHERE rp."RoleId" = u."RoleId"))
                              OR (u."DesignationId" IS NOT NULL AND p."Id" IN (
                                  SELECT dp."PermissionId" FROM departmentpermissions dp 
                                  INNER JOIN designations d ON d."DepartmentId" = dp."DepartmentId" AND d."DeletedFlag" = 1 
                                  WHERE d."Id" = u."DesignationId"
                              ))
                              OR p."Id" IN (SELECT up."PermissionId" FROM userpermissions up WHERE up."UserId" = u."Id")
                          )
                    ), '') AS "PermissionsCsv",
                    COALESCE((
                        SELECT STRING_AGG(DISTINCT m.label, ',')
                        FROM menus m
                        WHERE m.deletedflag = 1
                          AND (
                              u."RoleId" = 2
                              OR m.permissionkey IS NULL 
                              OR m.permissionkey = ''
                              OR m.permissionkey IN (
                                  SELECT p."PermissionKey" FROM permissions p
                                  WHERE p."DeletedFlag" = 1
                                    AND (
                                        (u."RoleId" IS NOT NULL AND p."Id" IN (SELECT rp."PermissionId" FROM rolepermissions rp WHERE rp."RoleId" = u."RoleId"))
                                        OR (u."DesignationId" IS NOT NULL AND p."Id" IN (
                                            SELECT dp."PermissionId" FROM departmentpermissions dp 
                                            INNER JOIN designations d ON d."DepartmentId" = dp."DepartmentId" AND d."DeletedFlag" = 1 
                                            WHERE d."Id" = u."DesignationId"
                                        ))
                                        OR p."Id" IN (SELECT up."PermissionId" FROM userpermissions up WHERE up."UserId" = u."Id")
                                    )
                              )
                          )
                    ), '') AS "MenuNamesCsv"
                FROM users u
                LEFT JOIN roles r ON r."Id" = u."RoleId" AND r."DeletedFlag" = 1
                LEFT JOIN designations des ON des."Id" = u."DesignationId" AND des."DeletedFlag" = 1
                LEFT JOIN departments dept ON dept."Id" = des."DepartmentId" AND dept."DeletedFlag" = 1
                WHERE LOWER(u."Email") = {0}
                LIMIT 1
                """);

            return await _context.Database.SqlQueryRaw<UserLoginDetailsModel>(sql.ToString(), normalizedEmail)
                .SingleOrDefaultAsync();
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                FROM users
                ORDER BY "Id"
                """);

            return await _context.Users
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UserModel?> GetUserByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                FROM users
                WHERE "Id" = {0}
                """);

            return await _context.Users
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<bool> SetDeletedFlagAsync(int id, int deletedFlag)
        {
            var sql = new StringBuilder("""
                UPDATE users SET "DeletedFlag" = {0} WHERE "Id" = {1}
                """);
            var rows = await _context.Database.ExecuteSqlRawAsync(sql.ToString(), deletedFlag, id);
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

            // Dynamically check if the user's role is a Super Admin in database
            if (userRecord.RoleId.HasValue && await _context.Roles.AnyAsync(r => r.Id == userRecord.RoleId.Value && r.DeletedFlag == 1 && (r.IsSuperAdmin || r.Name.ToLower() == "super admin")))
            {
                return true;
            }

            var roleId = userRecord.RoleId ?? 0;
            var designationId = userRecord.DesignationId ?? 0;

            var sql = new StringBuilder("""
                SELECT DISTINCT p."PermissionKey" AS "Value"
                FROM permissions p
                WHERE p."DeletedFlag" = 1
                  AND (
                      ({0} > 0 AND p."Id" IN (
                          SELECT rp."PermissionId" 
                          FROM rolepermissions rp 
                          WHERE rp."RoleId" = {0} AND (rp."Access" IS NULL OR rp."Access" = 'Allow')
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
                """);

            var permissions = await _context.Database.SqlQueryRaw<string>(sql.ToString(), roleId, designationId, userId).ToListAsync();

            return permissionKeys.Any(k => permissions.Contains(k, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<List<string>> GetUserPermissionKeysAsync(int userId, int? roleId = null, int? designationId = null)
        {
            var rId = roleId ?? 0;
            var dId = designationId ?? 0;

            if ((rId == 0 || dId == 0) && userId > 0)
            {
                var userRecord = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId && u.DeletedFlag == 1)
                    .Select(u => new { u.RoleId, u.DesignationId })
                    .FirstOrDefaultAsync();

                if (userRecord is null && rId == 0) return [];
                if (userRecord != null)
                {
                    if (rId == 0) rId = userRecord.RoleId ?? 0;
                    if (dId == 0) dId = userRecord.DesignationId ?? 0;
                }
            }

            // Dynamically check if the role has Super Admin permissions in database
            if (rId > 0 && await _context.Roles.AnyAsync(r => r.Id == rId && r.DeletedFlag == 1 && (r.IsSuperAdmin || r.Name.ToLower() == "super admin")))
            {
                var superAdminSql = new StringBuilder("""
                    SELECT DISTINCT p."PermissionKey" AS "Value"
                    FROM permissions p
                    WHERE p."DeletedFlag" = 1
                    ORDER BY "Value" ASC
                """);

                return await _context.Database.SqlQueryRaw<string>(superAdminSql.ToString()).ToListAsync();
            }

            var sql = new StringBuilder("""
                SELECT DISTINCT p."PermissionKey" AS "Value"
                FROM permissions p
                WHERE p."DeletedFlag" = 1
                  AND (
                      ({0} > 0 AND p."Id" IN (
                          SELECT rp."PermissionId" 
                          FROM rolepermissions rp 
                          WHERE rp."RoleId" = {0} AND (rp."Access" IS NULL OR rp."Access" = 'Allow')
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
                """);

            return await _context.Database.SqlQueryRaw<string>(sql.ToString(), rId, dId, userId).ToListAsync();
        }

        public async Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash)
        {
            var sql = new StringBuilder("""
                UPDATE users SET "Password" = {0}, "IsFirstLogin" = false WHERE "Id" = {1}
                """);
            var rows = await _context.Database.ExecuteSqlRawAsync(sql.ToString(), newPasswordHash, userId);
            return rows > 0;
        }

        public async Task<Dictionary<int, string>> GetActiveRolesLookupAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "Description", "DeletedFlag", "ParentRoleId", "IsSuperAdmin", "IsSystemRole", "CreatedAt", "UpdatedAt"
                FROM roles
                WHERE "DeletedFlag" = 1
                ORDER BY "Id"
                """);

            var roles = await _context.Roles
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .Select(r => new { r.Id, r.Name })
                .ToDictionaryAsync(r => r.Id, r => r.Name);

            return roles;
        }

        public async Task<Dictionary<int, string>> GetActiveDesignationsLookupAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "DepartmentId", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM designations
                WHERE "DeletedFlag" = 1
                ORDER BY "Name"
                """);

            var designations = await _context.Designations
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .Select(d => new { d.Id, d.Name })
                .ToDictionaryAsync(d => d.Id, d => d.Name);

            return designations;
        }

        public async Task<string?> GetRoleNameByIdAsync(int roleId)
        {
            var sql = new StringBuilder("""
                SELECT "Name" AS "Value"
                FROM roles
                WHERE "Id" = {0} AND "DeletedFlag" = 1
                """);

            return await _context.Database.SqlQueryRaw<string>(sql.ToString(), roleId).FirstOrDefaultAsync();
        }

        public async Task<string?> GetDesignationNameByIdAsync(int designationId)
        {
            var sql = new StringBuilder("""
                SELECT "Name" AS "Value"
                FROM designations
                WHERE "Id" = {0} AND "DeletedFlag" = 1
                """);

            return await _context.Database.SqlQueryRaw<string>(sql.ToString(), designationId).FirstOrDefaultAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            var sql = new StringBuilder();
            if (excludeUserId.HasValue)
            {
                sql.Append("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM users
                    WHERE LOWER("Email") = LOWER({0}) AND "DeletedFlag" = 1 AND "Id" <> {1}
                """);
                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), email, excludeUserId.Value).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                sql.Append("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM users
                    WHERE LOWER("Email") = LOWER({0}) AND "DeletedFlag" = 1
                """);
                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), email).SingleOrDefaultAsync();
                return count > 0;
            }
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null)
        {
            var sql = new StringBuilder();
            if (excludeUserId.HasValue)
            {
                sql.Append("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM users
                    WHERE "Phone" = {0} AND "DeletedFlag" = 1 AND "Id" <> {1}
                """);
                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), phone, excludeUserId.Value).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                sql.Append("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM users
                    WHERE "Phone" = {0} AND "DeletedFlag" = 1
                """);
                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), phone).SingleOrDefaultAsync();
                return count > 0;
            }
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            var sql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1
            """);

            return await _context.Database.SqlQueryRaw<int>(sql.ToString()).SingleOrDefaultAsync();
        }

        public async Task<int> GetUsersWithRoleCountAsync()
        {
            var sql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1 AND "RoleId" IS NOT NULL
            """);

            return await _context.Database.SqlQueryRaw<int>(sql.ToString()).SingleOrDefaultAsync();
        }

        public async Task<List<string>> GetUserPermissionKeysForProfileAsync(int roleId, int designationId)
        {
            if (roleId > 0 && await _context.Roles.AnyAsync(r => r.Id == roleId && r.DeletedFlag == 1 && (r.IsSuperAdmin || r.Name.ToLower() == "super admin")))
            {
                var superAdminSql = new StringBuilder("""
                    SELECT "PermissionKey" AS "Value"
                    FROM permissions
                    WHERE "DeletedFlag" = 1
                    ORDER BY "Id"
                """);

                return await _context.Database.SqlQueryRaw<string>(superAdminSql.ToString()).ToListAsync();
            }

            var sql = new StringBuilder("""
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
                """);

            return await _context.Database.SqlQueryRaw<string>(sql.ToString(), roleId, designationId).ToListAsync();
        }

        public async Task<Dictionary<int, string>> GetUserRoleMapAsync()
        {
            try
            {
                var roleSql = new StringBuilder("""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "ParentRoleId", "IsSuperAdmin", "IsSystemRole", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "DeletedFlag" = 1
                """);

                var roles = await _context.Roles
                    .FromSqlRaw(roleSql.ToString())
                    .AsNoTracking()
                    .Select(r => new { r.Id, r.Name })
                    .ToDictionaryAsync(r => r.Id, r => r.Name);

                var userRoleSql = new StringBuilder("""
                    SELECT "Id", "RoleId"
                    FROM users
                    WHERE "DeletedFlag" = 1 AND "RoleId" IS NOT NULL
                """);

                var userRoles = await _context.Users
                    .FromSqlRaw(userRoleSql.ToString())
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
