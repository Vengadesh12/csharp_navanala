using System.Text;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class DepartmentRepository : Repository<DepartmentModel>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<DepartmentModel>> GetActiveDepartmentsWithDesignationsAsync()
        {
            var deptSql = new StringBuilder("""
                SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM departments
                WHERE "DeletedFlag" = 1
                ORDER BY "Name" ASC
            """);

            return await _context.Departments
                .FromSqlRaw(deptSql.ToString())
                .Include(d => d.Designations.Where(des => des.DeletedFlag == 1))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DepartmentModel?> GetActiveDepartmentByIdAsync(int id)
        {
            var deptSql = new StringBuilder("""
                SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM departments
                WHERE "Id" = {0} AND "DeletedFlag" = 1
                LIMIT 1
            """);

            return await _context.Departments
                .FromSqlRaw(deptSql.ToString(), id)
                .Include(d => d.Designations.Where(des => des.DeletedFlag == 1))
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, string>> GetDepartmentNameDictionaryAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM departments
                WHERE "DeletedFlag" = 1
            """);

            var depts = await _context.Departments
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();

            return depts.ToDictionary(d => d.Id, d => d.Name);
        }

        public async Task<bool> DepartmentExistsByNameAsync(string name, int? excludeId = null)
        {
            var sql = new StringBuilder();
            if (excludeId.HasValue)
            {
                sql.Append("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM departments
                    WHERE "DeletedFlag" = 1 AND LOWER("Name") = LOWER({0}) AND "Id" <> {1}
                """);
                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), name.Trim(), excludeId.Value).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                sql.Append("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM departments
                    WHERE "DeletedFlag" = 1 AND LOWER("Name") = LOWER({0})
                """);
                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), name.Trim()).SingleOrDefaultAsync();
                return count > 0;
            }
        }
    }
}
