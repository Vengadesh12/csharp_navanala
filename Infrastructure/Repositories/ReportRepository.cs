using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
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

        public async Task<(List<ReportModel> Reports, int TotalReports, int ReadyReports, int TotalUsers, int UsersWithRole, List<ReportCategoryModel> Categories)> GetReportsOverviewDataAsync(string? category, string? search, CancellationToken cancellationToken = default)
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
                sql.Append($" AND LOWER(category) = LOWER({{{paramIndex++}}})");
                parameters.Add(category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                sql.Append($" AND (LOWER(title) LIKE {{{paramIndex}}} OR LOWER(description) LIKE {{{paramIndex}}} OR LOWER(created_by) LIKE {{{paramIndex++}}})");
                parameters.Add(pattern);
            }

            sql.Append(" ORDER BY id DESC");

            var rawReports = await _context.Reports
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var totalReportsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM reports
                WHERE deleted_flag = 1
            """);
            var totalReports = await _context.Database.SqlQueryRaw<int>(totalReportsSql.ToString()).SingleOrDefaultAsync(cancellationToken);

            var readyReportsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM reports
                WHERE deleted_flag = 1 AND (status = 'Ready' OR status = 'Generated')
            """);
            var readyReports = await _context.Database.SqlQueryRaw<int>(readyReportsSql.ToString()).SingleOrDefaultAsync(cancellationToken);

            var totalUsersSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1
            """);
            var totalUsers = await _context.Database.SqlQueryRaw<int>(totalUsersSql.ToString()).SingleOrDefaultAsync(cancellationToken);

            var usersWithRoleSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1 AND "RoleId" IS NOT NULL
            """);
            var usersWithRole = await _context.Database.SqlQueryRaw<int>(usersWithRoleSql.ToString()).SingleOrDefaultAsync(cancellationToken);

            var categoriesSql = new StringBuilder("""
                SELECT id, name, description, deleted_flag, created_at, updated_at
                FROM report_categories
                WHERE deleted_flag = 1
                ORDER BY name ASC
            """);

            var categories = await _context.ReportCategories
                .FromSqlRaw(categoriesSql.ToString())
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (rawReports, totalReports, readyReports, totalUsers, usersWithRole, categories);
        }

        public async Task<List<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default)
        {
            var catSql = new StringBuilder("""
                SELECT name AS "Value"
                FROM report_categories
                WHERE deleted_flag = 1
                ORDER BY name ASC
            """);

            var dbCategories = await _context.Database
                .SqlQueryRaw<string>(catSql.ToString())
                .ToListAsync(cancellationToken);

            if (dbCategories.Count > 0)
            {
                return dbCategories;
            }

            var distinctSql = new StringBuilder("""
                SELECT DISTINCT category AS "Value"
                FROM reports
                WHERE deleted_flag = 1 AND category IS NOT NULL AND category <> ''
                ORDER BY "Value" ASC
            """);

            return await _context.Database.SqlQueryRaw<string>(distinctSql.ToString()).ToListAsync(cancellationToken);
        }

        public async Task<ReportModel?> GetReportByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var sql = new StringBuilder("""
                SELECT id, title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag
                FROM reports
                WHERE id = {0} AND deleted_flag = 1
            """);

            return await _context.Reports
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ReportModel> AddReportAsync(ReportModel report, CancellationToken cancellationToken = default)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync(cancellationToken);
            return report;
        }

        public async Task<ReportModel> CreateReportRecordAsync(string title, string description, int? categoryId, string categoryName, string format, string creatorName, string fileSize, string? storedFileName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = DateTime.UtcNow;

            var insertSql = new StringBuilder("""
                INSERT INTO reports (title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {9}, 1)
                RETURNING id AS "Value"
            """);

            var newId = _context.Database.SqlQueryRaw<int>(
                insertSql.ToString(),
                title, description, (object?)categoryId ?? DBNull.Value, categoryName, format, creatorName, "Ready", fileSize, (object?)storedFileName ?? DBNull.Value, now)
            .AsEnumerable()
            .Single();

            var report = await GetReportByIdAsync(newId, cancellationToken);
            return report ?? new ReportModel
            {
                Id = newId,
                Title = title,
                Description = description,
                CategoryId = categoryId,
                Category = categoryName,
                Format = format,
                CreatedBy = creatorName,
                Status = "Ready",
                FileSize = fileSize,
                FileName = storedFileName,
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        public async Task<ReportModel?> UpdateReportRecordAsync(int id, string title, string description, int? categoryId, string categoryName, string format, string? status, string? newFileName, string? newFileSize, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var updateSql = new StringBuilder("""
                UPDATE reports
                SET title = {0}, description = {1}, category_id = {2}, category = {3}, format = {4}, status = COALESCE(NULLIF({5}, ''), status),
                    file_name = COALESCE({6}, file_name), file_size = COALESCE({7}, file_size), updated_at = {8}
                WHERE id = {9} AND deleted_flag = 1
            """);

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                updateSql.ToString(),
                new object[] { title, description, (object?)categoryId ?? DBNull.Value, categoryName, format, status ?? string.Empty, (object?)newFileName ?? DBNull.Value, (object?)newFileSize ?? DBNull.Value, now, id },
                cancellationToken);

            if (rowsAffected == 0) return null;

            return await GetReportByIdAsync(id, cancellationToken);
        }

        public async Task UpdateReportAsync(ReportModel report, CancellationToken cancellationToken = default)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> SoftDeleteReportAsync(int id, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var deleteSql = new StringBuilder("""
                UPDATE reports
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """);

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(deleteSql.ToString(), new object[] { now, id }, cancellationToken);

            return rowsAffected > 0;
        }

        public async Task<List<ReportCategoryModel>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, deleted_flag, created_at, updated_at
                FROM report_categories
                WHERE deleted_flag = 1
                ORDER BY name ASC
            """);

            return await _context.ReportCategories
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<ReportCategoryModel?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, deleted_flag, created_at, updated_at
                FROM report_categories
                WHERE id = {0} AND deleted_flag = 1
                LIMIT 1
            """);

            return await _context.ReportCategories
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ReportCategoryModel?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, deleted_flag, created_at, updated_at
                FROM report_categories
                WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
                LIMIT 1
            """);

            return await _context.ReportCategories
                .FromSqlRaw(sql.ToString(), name.Trim())
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> CategoryExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var sql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM report_categories
                WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
            """);

            var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), name.Trim()).SingleOrDefaultAsync(cancellationToken);

            return count > 0;
        }

        public async Task<ReportCategoryModel> AddCategoryAsync(ReportCategoryModel category, CancellationToken cancellationToken = default)
        {
            _context.ReportCategories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);
            return category;
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            var catSql = new StringBuilder("""
                SELECT id, name, description, deleted_flag, created_at, updated_at
                FROM report_categories
                WHERE id = {0} AND deleted_flag = 1
            """);

            var category = await _context.ReportCategories
                .FromSqlRaw(catSql.ToString(), id)
                .FirstOrDefaultAsync(cancellationToken);

            if (category == null) return false;

            category.DeletedFlag = 0;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
