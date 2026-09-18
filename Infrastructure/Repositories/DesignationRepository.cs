using System.Text;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class DesignationRepository : Repository<DesignationModel>, IDesignationRepository
    {
        public DesignationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<DesignationModel>> GetActiveDesignationsAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "DepartmentId", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM designations
                WHERE "DeletedFlag" = 1
                ORDER BY "Name"
            """);

            return await _context.Designations
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DesignationModel?> GetActiveDesignationByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "DepartmentId", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM designations
                WHERE "Id" = {0} AND "DeletedFlag" = 1
                LIMIT 1
            """);

            return await _context.Designations
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, string>> GetDesignationNameDictionaryAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "DepartmentId", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM designations
                WHERE "DeletedFlag" = 1
            """);

            var designations = await _context.Designations
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();

            return designations.ToDictionary(d => d.Id, d => d.Name);
        }

        public async Task<bool> DesignationExistsByNameAsync(string name, int? excludeId = null)
        {
            var trimmed = name.Trim();
            var sql = new StringBuilder();
            if (excludeId.HasValue)
            {
                sql.Append("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM designations
                    WHERE "DeletedFlag" = 1 AND LOWER("Name") = LOWER({0}) AND "Id" <> {1}
                """);
                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), trimmed, excludeId.Value).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                sql.Append("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM designations
                    WHERE "DeletedFlag" = 1 AND LOWER("Name") = LOWER({0})
                """);
                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), trimmed).SingleOrDefaultAsync();
                return count > 0;
            }
        }

        public async Task<string?> GetDepartmentNameByIdAsync(int departmentId)
        {
            var sql = new StringBuilder("""
                SELECT "Name" AS "Value"
                FROM departments
                WHERE "Id" = {0} AND "DeletedFlag" = 1
            """);

            return await _context.Database.SqlQueryRaw<string>(sql.ToString(), departmentId).FirstOrDefaultAsync();
        }

        public async Task<bool> SetDeletedFlagAsync(int id, int deletedFlag)
        {
            var sql = new StringBuilder("""
                UPDATE designations SET "DeletedFlag" = {0} WHERE "Id" = {1}
            """);

            var rows = await _context.Database.ExecuteSqlRawAsync(sql.ToString(), deletedFlag, id);
            return rows > 0;
        }

        public async Task<List<DesignationModel>> GetDesignationsByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return [];

            var sql = new StringBuilder("""
                SELECT "Id", "Name", "DepartmentId", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM designations
                WHERE "DeletedFlag" = 1 AND "Id" IN (
            """);

            var parameters = new List<object>();
            for (int i = 0; i < idList.Count; i++)
            {
                if (i > 0) sql.Append(", ");
                sql.Append($"{{{i}}}");
                parameters.Add(idList[i]);
            }
            sql.Append(") ORDER BY \"Name\"");

            return await _context.Designations
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<DesignationModel>> GetDesignationsByDepartmentIdAsync(int departmentId)
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "DepartmentId", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM designations
                WHERE "DepartmentId" = {0} AND "DeletedFlag" = 1
                ORDER BY "Name"
            """);

            return await _context.Designations
                .FromSqlRaw(sql.ToString(), departmentId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
