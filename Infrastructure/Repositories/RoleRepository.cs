using System.Text;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class RoleRepository : Repository<RoleModel>, IRoleRepository
    {
        public RoleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<RoleModel>> GetActiveRolesAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "Description", "DeletedFlag", "ParentRoleId", "IsSuperAdmin", "IsSystemRole", "CreatedAt", "UpdatedAt"
                FROM roles
                WHERE "DeletedFlag" = 1
                ORDER BY "Id"
            """);

            return await _context.Roles
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RoleModel?> GetActiveRoleByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "Description", "DeletedFlag", "ParentRoleId", "IsSuperAdmin", "IsSystemRole", "CreatedAt", "UpdatedAt"
                FROM roles
                WHERE "Id" = {0} AND "DeletedFlag" = 1
                LIMIT 1
            """);

            return await _context.Roles
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SetDeletedFlagAsync(int id, int deletedFlag)
        {
            var sql = new StringBuilder("""
                UPDATE roles SET "DeletedFlag" = {0} WHERE "Id" = {1}
            """);

            var rows = await _context.Database.ExecuteSqlRawAsync(sql.ToString(), deletedFlag, id);
            return rows > 0;
        }

        public async Task<Dictionary<int, string>> GetRoleNameDictionaryAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "Description", "DeletedFlag", "ParentRoleId", "IsSuperAdmin", "IsSystemRole", "CreatedAt", "UpdatedAt"
                FROM roles
                WHERE "DeletedFlag" = 1
            """);

            var roles = await _context.Roles
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();

            return roles.ToDictionary(r => r.Id, r => r.Name);
        }
    }
}
