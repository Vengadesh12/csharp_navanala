using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IApplicationDbContext _context;
        private readonly IWebHostEnvironment? _environment;

        public ReportService(IApplicationDbContext context, IWebHostEnvironment? environment = null)
        {
            _context = context;
            _environment = environment;
        }

        private string GetReportDirectory()
        {
            var root = _environment?.ContentRootPath ?? Directory.GetCurrentDirectory();
            var dir = Path.Combine(root, "report");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public async Task<ReportsOverviewResponse> GetReportsAsync(string? category, string? search)
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
                Reports = rawReports.ToDtoList(),
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
                    SELECT id, title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag
                    FROM reports
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (report == null) return null;

            // If a real file was uploaded and saved in the report/ directory, serve it!
            if (!string.IsNullOrWhiteSpace(report.FileName))
            {
                var reportDir = GetReportDirectory();
                var filePath = Path.Combine(reportDir, report.FileName);
                if (File.Exists(filePath))
                {
                    var fileBytes = await File.ReadAllBytesAsync(filePath);
                    var ext = Path.GetExtension(report.FileName).ToLowerInvariant();
                    var contentType = ext switch
                    {
                        ".pdf" => "application/pdf",
                        ".csv" => "text/csv",
                        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        ".xls" => "application/vnd.ms-excel",
                        ".json" => "application/json",
                        ".doc" => "application/msword",
                        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        ".txt" => "text/plain",
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".zip" => "application/zip",
                        _ => "application/octet-stream"
                    };

                    // Extract original clean file name if prefixed with timestamp & guid
                    var downloadFileName = report.FileName;
                    var parts = report.FileName.Split('_', 3);
                    if (parts.Length == 3 && long.TryParse(parts[0], out _))
                    {
                        downloadFileName = parts[2];
                    }
                    else
                    {
                        var safeTitle = string.Concat(report.Title.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
                        if (!string.IsNullOrWhiteSpace(safeTitle))
                        {
                            downloadFileName = $"{safeTitle}{ext}";
                        }
                    }

                    return new ReportDownloadResult
                    {
                        FileBytes = fileBytes,
                        ContentType = contentType,
                        FileName = downloadFileName
                    };
                }
            }

            // Fallback for mock/seed reports without physical disk files
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

        public async Task<ReportDto> CreateReportAsync(CreateReportRequest request, string creatorName)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BadRequestException("Report title is required.");
            }

            var title = request.Title.Trim();
            var description = request.Description.Trim();
            var format = string.IsNullOrWhiteSpace(request.Format) ? "PDF" : request.Format.Trim();
            var now = DateTime.UtcNow;

            string? storedFileName = null;
            string fileSize = "1.5 MB";

            // Process uploaded file (must be less than 5 MB)
            if (request.File != null && request.File.Length > 0)
            {
                if (request.File.Length > 5 * 1024 * 1024)
                {
                    throw new BadRequestException("Report file size cannot exceed 5 MB.");
                }

                var reportDir = GetReportDirectory();
                var originalFileName = Path.GetFileName(request.File.FileName);
                var extension = Path.GetExtension(originalFileName);
                var rawName = Path.GetFileNameWithoutExtension(originalFileName);
                var cleanName = string.Concat(rawName.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));
                if (string.IsNullOrWhiteSpace(cleanName)) cleanName = "report";

                fileSize = request.File.Length >= 1024 * 1024
                    ? $"{(request.File.Length / (1024.0 * 1024.0)):F1} MB"
                    : $"{(request.File.Length / 1024.0):F0} KB";

                if (string.IsNullOrWhiteSpace(request.Format) || request.Format == "PDF")
                {
                    var extClean = extension.TrimStart('.').ToLowerInvariant();
                    format = extClean switch
                    {
                        "pdf" => "PDF",
                        "csv" => "CSV",
                        "xlsx" or "xls" => "Excel",
                        "json" => "JSON",
                        "docx" or "doc" => "Word",
                        "txt" => "TXT",
                        _ => string.IsNullOrWhiteSpace(extClean) ? format : extClean.ToUpperInvariant()
                    };
                }

                var uniqueFileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid().ToString("N")[..6]}_{cleanName}{extension}";
                var destinationPath = Path.Combine(reportDir, uniqueFileName);

                using (var stream = new FileStream(destinationPath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                storedFileName = uniqueFileName;
            }

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

            var newId = _context.Database.SqlQueryRaw<int>("""
                INSERT INTO reports (title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {9}, 1)
                RETURNING id AS "Value"
            """, title, description, (object?)categoryId ?? DBNull.Value, categoryName, format, creatorName, "Ready", fileSize, (object?)storedFileName ?? DBNull.Value, now).AsEnumerable().Single();

            var report = await _context.Reports
                .FromSqlRaw("""
                    SELECT id, title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag
                    FROM reports
                    WHERE id = {0} AND deleted_flag = 1
                """, newId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return report!.ToDto();
        }

        public async Task<ReportDto?> UpdateReportAsync(int id, UpdateReportRequest request)
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

            string? newFileName = null;
            string? newFileSize = null;
            if (request.File != null && request.File.Length > 0)
            {
                if (request.File.Length > 5 * 1024 * 1024)
                {
                    throw new BadRequestException("Report file size cannot exceed 5 MB.");
                }

                var reportDir = GetReportDirectory();
                var originalFileName = Path.GetFileName(request.File.FileName);
                var extension = Path.GetExtension(originalFileName);
                var rawName = Path.GetFileNameWithoutExtension(originalFileName);
                var cleanName = string.Concat(rawName.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));
                if (string.IsNullOrWhiteSpace(cleanName)) cleanName = "report";

                newFileSize = request.File.Length >= 1024 * 1024
                    ? $"{(request.File.Length / (1024.0 * 1024.0)):F1} MB"
                    : $"{(request.File.Length / 1024.0):F0} KB";

                var uniqueFileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid().ToString("N")[..6]}_{cleanName}{extension}";
                var destinationPath = Path.Combine(reportDir, uniqueFileName);

                using (var stream = new FileStream(destinationPath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                newFileName = uniqueFileName;
            }

            var now = DateTime.UtcNow;
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE reports
                SET title = {0}, description = {1}, category_id = {2}, category = {3}, format = {4}, status = COALESCE(NULLIF({5}, ''), status),
                    file_name = COALESCE({6}, file_name), file_size = COALESCE({7}, file_size), updated_at = {8}
                WHERE id = {9} AND deleted_flag = 1
            """, request.Title.Trim(), request.Description.Trim(), (object?)categoryId ?? DBNull.Value, categoryName, request.Format.Trim(), request.Status ?? string.Empty, (object?)newFileName ?? DBNull.Value, (object?)newFileSize ?? DBNull.Value, now, id);

            if (rowsAffected == 0) return null;

            var updated = await _context.Reports
                .FromSqlRaw("""
                    SELECT id, title, description, category_id, category, format, created_by, status, file_size, file_name, created_at, updated_at, deleted_flag
                    FROM reports
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return updated?.ToDto();
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var now = DateTime.UtcNow;
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE reports
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """, now, id);

            return rowsAffected > 0;
        }
    }
}
