using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Report> Reports, int TotalReports, int ReadyReports, int TotalUsers, int UsersWithRole, List<ReportCategory> Categories)> GetReportsOverviewDataAsync(string? category, string? search)
        {
            var sql = new StringBuilder("""
                SELECT id, title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag
                FROM reports
                WHERE deleted_flag = 1
            """);

            var parameters = new List<object>();
            int paramIndex = 0;

            if (!string.IsNullOrWhiteSpace(category) && category != "ALL")
            {
                if (int.TryParse(category, out int catId))
                {
                    sql.Append($" AND (category_id = {{{paramIndex++}}} OR LOWER(category) = LOWER({{{paramIndex++}}}))");
                    parameters.Add(catId);
                    parameters.Add(category.Trim());
                }
                else
                {
                    sql.Append($" AND LOWER(category) = LOWER({{{paramIndex++}}})");
                    parameters.Add(category.Trim());
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                sql.Append($" AND (LOWER(title) LIKE {{{paramIndex}}} OR LOWER(description) LIKE {{{paramIndex}}} OR LOWER(category) LIKE {{{paramIndex++}}})");
                parameters.Add(pattern);
            }

            sql.Append(" ORDER BY id DESC");

            var rawReports = await _context.Reports
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            var totalReports = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM reports
                WHERE deleted_flag = 1
            """).SingleOrDefaultAsync();

            var readyReports = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM reports
                WHERE deleted_flag = 1 AND (status = 'Ready' OR status = 'Generated')
            """).SingleOrDefaultAsync();

            var totalUsers = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1
            """).SingleOrDefaultAsync();

            var usersWithRole = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1 AND "RoleId" IS NOT NULL
            """).SingleOrDefaultAsync();

            var categories = await _context.ReportCategories
                .Where(c => c.DeletedFlag == 1)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            return (rawReports, totalReports, readyReports, totalUsers, usersWithRole, categories);
        }

        public async Task<List<string>> GetCategoryNamesAsync()
        {
            var dbCategories = await _context.ReportCategories
                .Where(c => c.DeletedFlag == 1)
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            if (dbCategories.Count > 0)
            {
                return dbCategories;
            }

            return await _context.Database.SqlQueryRaw<string>("""
                SELECT DISTINCT category AS "Value"
                FROM reports
                WHERE deleted_flag = 1 AND category IS NOT NULL AND category <> ''
                ORDER BY "Value" ASC
            """).ToListAsync();
        }

        public async Task<Report?> GetReportByIdAsync(int id)
        {
            return await _context.Reports
                .FromSqlRaw("""
                    SELECT id, title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag
                    FROM reports
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<Report> AddReportAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<Report> CreateReportRecordAsync(string title, string description, int? categoryId, string categoryName, string format, string creatorName, string fileSize, string? storedFileName)
        {
            var now = DateTime.UtcNow;
            var newId = await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO reports (title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {9}, 1)
                RETURNING id AS "Value"
            """, title, description, (object?)categoryId ?? DBNull.Value, categoryName, format, creatorName, "Ready", fileSize, (object?)storedFileName ?? DBNull.Value, now).SingleAsync();

            var report = await GetReportByIdAsync(newId);
            return report!;
        }

        public async Task<Report?> UpdateReportRecordAsync(int id, string title, string description, int? categoryId, string categoryName, string format, string? status, string? newFileName, string? newFileSize)
        {
            var now = DateTime.UtcNow;
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE reports
                SET title = {0}, description = {1}, category_id = {2}, category = {3}, format = {4}, status = COALESCE(NULLIF({5}, ''), status),
                    file_name = COALESCE({6}, file_name), file_size = COALESCE({7}, file_size), updated_at = {8}
                WHERE id = {9} AND deleted_flag = 1
            """, title, description, (object?)categoryId ?? DBNull.Value, categoryName, format, status ?? string.Empty, (object?)newFileName ?? DBNull.Value, (object?)newFileSize ?? DBNull.Value, now, id);

            if (rowsAffected == 0) return null;

            return await GetReportByIdAsync(id);
        }

        public async Task UpdateReportAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteReportAsync(int id)
        {
            var now = DateTime.UtcNow;
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE reports
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """, now, id);

            return rowsAffected > 0;
        }

        public async Task<List<ReportCategory>> GetAllCategoriesAsync()
        {
            return await _context.ReportCategories
                .FromSqlRaw("""
                    SELECT id, name, description, deleted_flag, created_at, updated_at
                    FROM report_categories
                    WHERE deleted_flag = 1
                    ORDER BY name ASC
                """)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ReportCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.ReportCategories
                .FromSqlRaw("""
                    SELECT id, name, description, deleted_flag, created_at, updated_at
                    FROM report_categories
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<ReportCategory?> GetCategoryByNameAsync(string name)
        {
            return await _context.ReportCategories
                .FirstOrDefaultAsync(c => c.DeletedFlag == 1 && c.Name.ToLower() == name.Trim().ToLower());
        }

        public async Task<bool> CategoryExistsByNameAsync(string name)
        {
            var count = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM report_categories
                WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
            """, name.Trim()).SingleOrDefaultAsync();

            return count > 0;
        }

        public async Task<ReportCategory> AddCategoryAsync(ReportCategory category)
        {
            _context.ReportCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var category = await _context.ReportCategories.FirstOrDefaultAsync(c => c.Id == id && c.DeletedFlag == 1);
            if (category == null) return false;

            category.DeletedFlag = 0;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
