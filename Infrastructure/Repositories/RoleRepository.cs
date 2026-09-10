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
            return await _context.Roles
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "ParentRoleId", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "DeletedFlag" = 1
                    ORDER BY "Id"
                    """)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RoleModel?> GetActiveRoleByIdAsync(int id)
        {
            return await _context.Roles
                .FromSqlInterpolated($"""
                    SELECT "Id", "Name", "Description", "ParentRoleId", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "Id" = {id} AND "DeletedFlag" = 1
                    """)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<bool> SetDeletedFlagAsync(int id, int deletedFlag)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE roles SET "DeletedFlag" = {deletedFlag} WHERE "Id" = {id}
                """);
            return rows > 0;
        }

        public async Task<Dictionary<int, string>> GetRoleNameDictionaryAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .ToDictionaryAsync(r => r.Id, r => r.Name);
        }
    }
}
