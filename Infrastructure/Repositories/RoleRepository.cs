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
                .AsNoTracking()
                .Where(r => r.DeletedFlag == 1)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<RoleModel?> GetActiveRoleByIdAsync(int id)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.DeletedFlag == 1);
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
