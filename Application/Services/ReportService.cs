using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IHostEnvironment? _environment;

        public ReportService(IReportRepository reportRepository, IHostEnvironment? environment = null)
        {
            _reportRepository = reportRepository;
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
            var (rawReports, totalReports, readyReports, totalUsers, usersWithRole, categories) =
                await _reportRepository.GetReportsOverviewDataAsync(category, search);

            var coveragePercentage = totalUsers > 0 ? Math.Round((double)usersWithRole / totalUsers * 100) : 100;

            var categoryDtos = categories.Select(c => new ReportCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description ?? string.Empty,
                DeletedFlag = c.DeletedFlag,
                CreatedAt = c.CreatedAt
            }).ToList();

            return new ReportsOverviewResponse
            {
                ReportsGenerated = totalReports,
                ExportsReady = readyReports,
                RoleCoverage = $"{coveragePercentage}%",
                Reports = rawReports.ToDtoList(),
                Categories = categoryDtos
            };
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _reportRepository.GetCategoryNamesAsync();
        }

        public async Task<ReportDownloadResult?> GetReportDownloadAsync(int id)
        {
            var report = await _reportRepository.GetReportByIdAsync(id);
            if (report == null) return null;

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

            string? storedFileName = null;
            string fileSize = "1.5 MB";

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

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var catEntity = await _reportRepository.GetCategoryByIdAsync(categoryId.Value);
                if (catEntity != null)
                {
                    categoryName = catEntity.Name;
                }
            }
            else if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var catEntity = await _reportRepository.GetCategoryByNameAsync(categoryName);
                if (catEntity != null)
                {
                    categoryId = catEntity.Id;
                    categoryName = catEntity.Name;
                }
                else
                {
                    var newCat = ReportCategory.Create(categoryName, $"{categoryName} reports");
                    var addedCat = await _reportRepository.AddCategoryAsync(newCat);
                    categoryId = addedCat.Id;
                }
            }

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                categoryName = "Compliance";
            }

            var report = await _reportRepository.CreateReportRecordAsync(
                title,
                description,
                categoryId,
                categoryName,
                format,
                creatorName,
                fileSize,
                storedFileName);

            return report.ToDto();
        }

        public async Task<ReportDto?> UpdateReportAsync(int id, UpdateReportRequest request)
        {
            int? categoryId = request.CategoryId;
            string categoryName = request.Category?.Trim() ?? string.Empty;

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var catEntity = await _reportRepository.GetCategoryByIdAsync(categoryId.Value);
                if (catEntity != null)
                {
                    categoryName = catEntity.Name;
                }
            }
            else if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var catEntity = await _reportRepository.GetCategoryByNameAsync(categoryName);
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

            var updated = await _reportRepository.UpdateReportRecordAsync(
                id,
                request.Title.Trim(),
                request.Description.Trim(),
                categoryId,
                categoryName,
                request.Format.Trim(),
                request.Status,
                newFileName,
                newFileSize);

            return updated?.ToDto();
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            return await _reportRepository.SoftDeleteReportAsync(id);
        }
    }
}
