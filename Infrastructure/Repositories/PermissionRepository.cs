using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class PermissionRepository : Repository<Permission>, IPermissionRepository
    {
        public PermissionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PermissionsMatrixResponse> GetPermissionsMatrixAsync()
        {
            var permissions = await _context.Database.SqlQueryRaw<PermissionDto>("""
                  SELECT p."PermissionKey", p."Name", p."Description",
                      CASE WHEN rp."RoleId" IS NULL THEN 0 ELSE 1 END AS "IsAssigned"
                  FROM permissions p
                  LEFT JOIN rolepermissions rp ON rp."PermissionId" = p."Id" AND rp."RoleId" = 2
                  WHERE p."DeletedFlag" = 1
                  ORDER BY p."Id"
                """).ToListAsync();

            var roles = await _context.Database.SqlQueryRaw<RolePermissionDto>("""
                  SELECT r."Id" AS "RoleId", r."Name" AS "RoleName",
                      COALESCE(STRING_AGG(p."PermissionKey", ',' ORDER BY p."Id"), '') AS "PermissionKeys"
                  FROM roles r
                  LEFT JOIN rolepermissions rp ON rp."RoleId" = r."Id"
                  LEFT JOIN permissions p ON p."Id" = rp."PermissionId" AND p."DeletedFlag" = 1
                  WHERE r."DeletedFlag" = 1
                  GROUP BY r."Id", r."Name"
                  ORDER BY r."Id"
                """).ToListAsync();

            var matrixRoles = roles.Select(role => new RolePermissionMatrixItem
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                PermissionKeys = string.IsNullOrWhiteSpace(role.PermissionKeys)
                    ? []
                    : role.PermissionKeys.Split(',', StringSplitOptions.RemoveEmptyEntries)
            }).ToList();

            var departments = await _context.Database.SqlQueryRaw<DepartmentPermissionDto>("""
                  SELECT d."Id" AS "DepartmentId", d."Name" AS "DepartmentName",
                      COALESCE(STRING_AGG(p."PermissionKey", ',' ORDER BY p."Id"), '') AS "PermissionKeys"
                  FROM departments d
                  LEFT JOIN departmentpermissions dp ON dp."DepartmentId" = d."Id"
                  LEFT JOIN permissions p ON p."Id" = dp."PermissionId" AND p."DeletedFlag" = 1
                  WHERE d."DeletedFlag" = 1
                  GROUP BY d."Id", d."Name"
                  ORDER BY d."Id"
                """).ToListAsync();

            var matrixDepts = departments.Select(dept => new DepartmentPermissionMatrixItem
            {
                DepartmentId = dept.DepartmentId,
                DepartmentName = dept.DepartmentName,
                PermissionKeys = string.IsNullOrWhiteSpace(dept.PermissionKeys)
                    ? []
                    : dept.PermissionKeys.Split(',', StringSplitOptions.RemoveEmptyEntries)
            }).ToList();

            return new PermissionsMatrixResponse
            {
                Permissions = permissions,
                Roles = matrixRoles,
                Departments = matrixDepts
            };
        }

        public async Task<List<PermissionDto>> GetAllActivePermissionsAsync()
        {
            return await _context.Database.SqlQueryRaw<PermissionDto>("""
                SELECT p."PermissionKey", p."Name", p."Description", 0 AS "IsAssigned"
                FROM permissions p
                WHERE p."DeletedFlag" = 1
                ORDER BY p."Id"
                """).ToListAsync();
        }

        public async Task<List<string>> GetPermissionKeysByRoleIdAsync(int roleId)
        {
            return await _context.Database.SqlQueryRaw<string>("""
                SELECT p."PermissionKey" AS "Value"
                FROM permissions p
                INNER JOIN rolepermissions rp ON rp."PermissionId" = p."Id"
                WHERE rp."RoleId" = {0} AND p."DeletedFlag" = 1
                ORDER BY p."Id"
                """, roleId).ToListAsync();
        }

        public async Task<bool> UpdateRolePermissionsAsync(int roleId, IEnumerable<string> permissionKeys)
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId && r.DeletedFlag == 1);
            if (!roleExists) return false;

            var validPermissionIds = await ResolvePermissionIdsAsync(permissionKeys);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Clear old assignments for this role
            await _context.Database.ExecuteSqlInterpolatedAsync($"""
                DELETE FROM rolepermissions WHERE "RoleId" = {roleId}
                """);

            // Insert new assignments
            foreach (var permissionId in validPermissionIds)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO rolepermissions ("RoleId", "PermissionId")
                    VALUES ({roleId}, {permissionId})
                    """);
            }

            await transaction.CommitAsync();
            return true;
        }

        public async Task<List<string>> GetPermissionKeysByDepartmentIdAsync(int departmentId)
        {
            return await _context.Database.SqlQueryRaw<string>("""
                SELECT p."PermissionKey" AS "Value"
                FROM permissions p
                INNER JOIN departmentpermissions dp ON dp."PermissionId" = p."Id"
                WHERE dp."DepartmentId" = {0} AND p."DeletedFlag" = 1
                ORDER BY p."Id"
                """, departmentId).ToListAsync();
        }

        public async Task<bool> UpdateDepartmentPermissionsAsync(int departmentId, IEnumerable<string> permissionKeys)
        {
            var deptExists = await _context.Departments.AnyAsync(d => d.Id == departmentId && d.DeletedFlag == 1);
            if (!deptExists) return false;

            var validPermissionIds = await ResolvePermissionIdsAsync(permissionKeys);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Clear old assignments for this department
            await _context.Database.ExecuteSqlInterpolatedAsync($"""
                DELETE FROM departmentpermissions WHERE "DepartmentId" = {departmentId}
                """);

            // Insert new assignments
            foreach (var permissionId in validPermissionIds)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO departmentpermissions ("DepartmentId", "PermissionId")
                    VALUES ({departmentId}, {permissionId})
                    """);
            }

            await transaction.CommitAsync();
            return true;
        }

        private async Task<List<int>> ResolvePermissionIdsAsync(IEnumerable<string> permissionKeys)
        {
            var validKeys = (permissionKeys ?? Enumerable.Empty<string>())
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Select(key => key.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            var permissionMap = await _context.Permissions
                .Where(p => p.DeletedFlag == 1)
                .ToDictionaryAsync(p => p.PermissionKey.ToLower(), p => p.Id);

            var validPermissionIds = new List<int>();
            foreach (var key in validKeys)
            {
                if (!permissionMap.TryGetValue(key, out var permissionId))
                {
                    var permName = FormatPermissionName(key);
                    var permDesc = $"Allows access to {key} capability.";
                    
                    var newPerm = new Permission
                    {
                        PermissionKey = key,
                        Name = permName,
                        Description = permDesc,
                        DeletedFlag = 1
                    };
                    _context.Permissions.Add(newPerm);
                    await _context.SaveChangesAsync();
                    
                    permissionId = newPerm.Id;
                    permissionMap[key] = permissionId;
                }
                validPermissionIds.Add(permissionId);
            }
            return validPermissionIds;
        }

        private static string FormatPermissionName(string key)
        {
            var parts = key.Split('.', '_', '-');
            return string.Join(" ", parts.Select(p => p.Length > 0 ? char.ToUpper(p[0]) + p[1..] : p));
        }
    }
}
