using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;

namespace MyBackend.Application.Services
{
    public class MenuService : IMenuService
    {
        private const int SuperAdminRoleId = 2;
        private readonly IApplicationDbContext _context;

        public MenuService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MenuItemDto>> GetUserMenusAsync(int userId)
        {
            var user = await _context.Users
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Email", "Password", "RoleId", "DesignationId", "Phone", "Age", "Address", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM users
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, userId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (user is null)
            {
                return [];
            }

            if (user.RoleId == SuperAdminRoleId)
            {
                // Super Admin has access to all active menus
                var rawMenus = await _context.Menus
                    .FromSqlRaw("""
                        SELECT id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag, created_at, updated_at
                        FROM menus
                        WHERE deletedflag = 1
                        ORDER BY orderindex ASC, id ASC
                    """)
                    .AsNoTracking()
                    .ToListAsync();

                return rawMenus.ToDtoList();
            }
            else
            {
                var roleId = user.RoleId ?? 0;
                var designationId = user.DesignationId ?? 0;

                // Get role + department permissions for regular role and filter menus
                var rawMenus = await _context.Menus
                    .FromSqlRaw("""
                        SELECT m.id, m.menukey, m.label, m.icon, m.route, m.groupname, m.description, m.orderindex, m.permissionkey, m.deletedflag, m.created_at, m.updated_at
                        FROM menus m
                        WHERE m.deletedflag = 1
                          AND (
                            m.permissionkey IS NULL 
                            OR m.permissionkey = '' 
                            OR m.permissionkey IN (
                                SELECT p."PermissionKey"
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
                            )
                          )
                        ORDER BY m.orderindex ASC, m.id ASC
                    """, roleId, designationId)
                    .AsNoTracking()
                    .ToListAsync();

                return rawMenus.ToDtoList();
            }
        }

        public async Task<List<MenuItemDto>> GetAllMenusAsync()
        {
            var rawMenus = await _context.Menus
                .FromSqlRaw("""
                    SELECT id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag, created_at, updated_at
                    FROM menus
                    WHERE deletedflag = 1
                    ORDER BY orderindex ASC, id ASC
                """)
                .AsNoTracking()
                .ToListAsync();

            return rawMenus.ToDtoList();
        }
    }
}
