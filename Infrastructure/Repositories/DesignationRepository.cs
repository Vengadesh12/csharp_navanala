using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class DesignationRepository : Repository<Designation>, IDesignationRepository
    {
        public DesignationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Designation>> GetActiveDesignationsAsync()
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(d => d.DeletedFlag == 1)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Designation?> GetActiveDesignationByIdAsync(int id)
        {
            return await _context.Designations
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && d.DeletedFlag == 1);
        }

        public async Task<Dictionary<int, string>> GetDesignationNameDictionaryAsync()
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(d => d.DeletedFlag == 1)
                .ToDictionaryAsync(d => d.Id, d => d.Name);
        }

        public async Task<bool> DesignationExistsByNameAsync(string name, int? excludeId = null)
        {
            var trimmed = name.Trim();
            if (excludeId.HasValue)
            {
                var count = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM designations
                    WHERE "DeletedFlag" = 1 AND LOWER("Name") = LOWER({0}) AND "Id" <> {1}
                """, trimmed, excludeId.Value).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                var count = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM designations
                    WHERE "DeletedFlag" = 1 AND LOWER("Name") = LOWER({0})
                """, trimmed).SingleOrDefaultAsync();
                return count > 0;
            }
        }

        public async Task<string?> GetDepartmentNameByIdAsync(int departmentId)
        {
            return await _context.Database.SqlQueryRaw<string>("""
                SELECT "Name" AS "Value"
                FROM departments
                WHERE "Id" = {0} AND "DeletedFlag" = 1
            """, departmentId).FirstOrDefaultAsync();
        }

        public async Task<bool> SetDeletedFlagAsync(int id, int deletedFlag)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE designations SET "DeletedFlag" = {deletedFlag} WHERE "Id" = {id}
                """);
            return rows > 0;
        }

        public async Task<List<Designation>> GetDesignationsByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return [];

            return await _context.Designations
                .Where(d => idList.Contains(d.Id) && d.DeletedFlag == 1)
                .ToListAsync();
        }

        public async Task<List<Designation>> GetDesignationsByDepartmentIdAsync(int departmentId)
        {
            return await _context.Designations
                .Where(d => d.DepartmentId == departmentId)
                .ToListAsync();
        }
    }
}
