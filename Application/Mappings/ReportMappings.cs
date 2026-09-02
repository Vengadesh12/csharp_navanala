using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Contracts;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class ReportMappings
    {
        public static ReportDto ToDto(this Report report)
        {
            return new ReportDto
            {
                Id = report.Id,
                Title = report.Title,
                Description = report.Description ?? string.Empty,
                CategoryId = report.CategoryId,
                Category = report.Category,
                Format = report.Format,
                CreatedBy = report.CreatedBy ?? "System Admin",
                Status = report.Status,
                FileSize = report.FileSize ?? "1.2 MB",
                FileName = report.FileName,
                CreatedAt = report.CreatedAt,
                DeletedFlag = report.DeletedFlag
            };
        }

        public static List<ReportDto> ToDtoList(this IEnumerable<Report> reports)
        {
            return reports.Select(r => r.ToDto()).ToList();
        }

        public static ReportCategoryDto ToDto(this ReportCategory cat)
        {
            return new ReportCategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                Description = cat.Description ?? string.Empty,
                DeletedFlag = cat.DeletedFlag,
                CreatedAt = cat.CreatedAt
            };
        }

        public static List<ReportCategoryDto> ToDtoList(this IEnumerable<ReportCategory> categories)
        {
            return categories.Select(c => c.ToDto()).ToList();
        }
    }
}
