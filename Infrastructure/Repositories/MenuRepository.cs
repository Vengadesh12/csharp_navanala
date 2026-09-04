using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _context;

        public MenuRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Menu>> GetAllActiveMenusAsync()
        {
            return await _context.Menus
                .FromSqlRaw("""
                    SELECT id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag, created_at, updated_at
                    FROM menus
                    WHERE deletedflag = 1
                    ORDER BY orderindex ASC, id ASC
                """)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Menu>> GetUserMenusAsync(int roleId, int designationId)
        {
            return await _context.Menus
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
        }
    }
}
