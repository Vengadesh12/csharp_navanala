using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.Interfaces;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IApplicationDbContext _context;

        public ReportService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportsOverviewResponse> GetReportsAsync(string? category, string? search)
        {
            var sql = new StringBuilder("""
                SELECT id, title, description, category_id, category, format, created_by, status, file_size, created_at, deleted_flag
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

            var reports = await _context.Reports
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

            var coveragePercentage = totalUsers > 0 ? Math.Round((double)usersWithRole / totalUsers * 100) : 100;

            var categories = await _context.ReportCategories
                .Where(c => c.DeletedFlag == 1)
                .OrderBy(c => c.Name)
                .Select(c => new ReportCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description ?? string.Empty,
                    DeletedFlag = c.DeletedFlag,
                    CreatedAt = c.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();

            return new ReportsOverviewResponse
            {
                ReportsGenerated = totalReports,
                ExportsReady = readyReports,
                RoleCoverage = $"{coveragePercentage}%",
                Reports = reports,
                Categories = categories
            };
        }

        public async Task<List<string>> GetCategoriesAsync()
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

        public async Task<ReportDownloadResult?> GetReportDownloadAsync(int id)
        {
            var report = await _context.Reports
                .FromSqlRaw("""
                    SELECT id, title, description, category_id, category, format, created_by, status, file_size, created_at, deleted_flag
                    FROM reports
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (report == null) return null;

            var format = (report.Format ?? "PDF").ToUpper();
            if (format == "JSON")
            {
                var jsonContent = JsonSerializer.Serialize(new
                {
                    report.Id,
                    report.Title,
                    report.CategoryId,
                    report.Category,
                    report.Description,
                    report.Format,
                    report.CreatedBy,
                    report.CreatedAt,
                    ExportTimestamp = DateTime.UtcNow,
                    System = "Enterprise RBAC Platform",
                    Status = "Certified"
                }, new JsonSerializerOptions { WriteIndented = true });

                return new ReportDownloadResult
                {
                    FileBytes = Encoding.UTF8.GetBytes(jsonContent),
                    ContentType = "application/json",
                    FileName = $"{report.Title.Replace(" ", "_")}.json"
                };
            }
            else if (format == "CSV")
            {
                var csv = $"Id,Title,CategoryId,Category,Format,CreatedBy,CreatedAt,Status\n{report.Id},\"{report.Title}\",{report.CategoryId?.ToString() ?? "null"},\"{report.Category}\",\"{report.Format}\",\"{report.CreatedBy}\",\"{report.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"Certified\"\n";
                return new ReportDownloadResult
                {
                    FileBytes = Encoding.UTF8.GetBytes(csv),
                    ContentType = "text/csv",
                    FileName = $"{report.Title.Replace(" ", "_")}.csv"
                };
            }
            else
            {
                var doc = $"=====================================================\n" +
                          $"               COMPLIANCE AUDIT REPORT               \n" +
                          $"=====================================================\n" +
                          $"Title:       {report.Title}\n" +
                          $"Category:    {report.Category} (ID: {report.CategoryId?.ToString() ?? "N/A"})\n" +
                          $"Generated By:{report.CreatedBy}\n" +
                          $"Date:        {report.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}\n" +
                          $"Status:      Verified & Certified\n" +
                          $"-----------------------------------------------------\n" +
                          $"Executive Summary & Audit Scope:\n" +
                          $"{report.Description}\n" +
                          $"=====================================================\n";
                return new ReportDownloadResult
                {
                    FileBytes = Encoding.UTF8.GetBytes(doc),
                    ContentType = "text/plain",
                    FileName = $"{report.Title.Replace(" ", "_")}.txt"
                };
            }
        }

        public async Task<Report> CreateReportAsync(CreateReportRequest request, string creatorName)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BadRequestException("Report title is required.");
            }

            var title = request.Title.Trim();
            var description = request.Description.Trim();
            var format = string.IsNullOrWhiteSpace(request.Format) ? "PDF" : request.Format.Trim();
            var now = DateTime.UtcNow;

            int? categoryId = request.CategoryId;
            string categoryName = request.Category?.Trim() ?? string.Empty;

            // If categoryId is provided, resolve category name from DB
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var catEntity = await _context.ReportCategories
                    .FirstOrDefaultAsync(c => c.Id == categoryId.Value && c.DeletedFlag == 1);
                if (catEntity != null)
                {
                    categoryName = catEntity.Name;
                }
            }
            else if (!string.IsNullOrWhiteSpace(categoryName))
            {
                // Find matching category by name or create if new
                var catEntity = await _context.ReportCategories
                    .FirstOrDefaultAsync(c => c.DeletedFlag == 1 && c.Name.ToLower() == categoryName.ToLower());
                if (catEntity != null)
                {
                    categoryId = catEntity.Id;
                    categoryName = catEntity.Name;
                }
                else
                {
                    // Create new category in DB automatically
                    var newCat = new ReportCategory
                    {
                        Name = categoryName,
                        Description = $"{categoryName} reports",
                        DeletedFlag = 1,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ReportCategories.Add(newCat);
                    await _context.SaveChangesAsync();
                    categoryId = newCat.Id;
                }
            }

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                categoryName = "Compliance";
            }

            var newId = await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO reports (title, description, category_id, category, format, created_by, status, file_size, created_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, 1)
                RETURNING id AS "Value"
            """, title, description, (object?)categoryId ?? DBNull.Value, categoryName, format, creatorName, "Ready", "1.5 MB", now).SingleAsync();

            var report = await _context.Reports
                .FromSqlRaw("""
                    SELECT id, title, description, category_id, category, format, created_by, status, file_size, created_at, deleted_flag
                    FROM reports
                    WHERE id = {0} AND deleted_flag = 1
                """, newId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return report!;
        }

        public async Task<Report?> UpdateReportAsync(int id, UpdateReportRequest request)
        {
            int? categoryId = request.CategoryId;
            string categoryName = request.Category?.Trim() ?? string.Empty;

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var catEntity = await _context.ReportCategories
                    .FirstOrDefaultAsync(c => c.Id == categoryId.Value && c.DeletedFlag == 1);
                if (catEntity != null)
                {
                    categoryName = catEntity.Name;
                }
            }
            else if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var catEntity = await _context.ReportCategories
                    .FirstOrDefaultAsync(c => c.DeletedFlag == 1 && c.Name.ToLower() == categoryName.ToLower());
                if (catEntity != null)
                {
                    categoryId = catEntity.Id;
                    categoryName = catEntity.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                categoryName = "Compliance";
            }

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE reports
                SET title = {0}, description = {1}, category_id = {2}, category = {3}, format = {4}, status = COALESCE(NULLIF({5}, ''), status)
                WHERE id = {6} AND deleted_flag = 1
            """, request.Title.Trim(), request.Description.Trim(), (object?)categoryId ?? DBNull.Value, categoryName, request.Format.Trim(), request.Status ?? string.Empty, id);

            if (rowsAffected == 0) return null;

            return await _context.Reports
                .FromSqlRaw("""
                    SELECT id, title, description, category_id, category, format, created_by, status, file_size, created_at, deleted_flag
                    FROM reports
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE reports
                SET deleted_flag = 0
                WHERE id = {0} AND deleted_flag = 1
            """, id);

            return rowsAffected > 0;
        }
    }
}
