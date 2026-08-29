using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    /// <summary>
    /// Implements specialized Role entity operations and queries.
    /// </summary>
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Role>> GetActiveRolesAsync()
        {
            return await _context.Roles
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "DeletedFlag" = 1
                    ORDER BY "Id"
                    """)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Role?> GetActiveRoleByIdAsync(int id)
        {
            return await _context.Roles
                .FromSqlInterpolated($"""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
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
