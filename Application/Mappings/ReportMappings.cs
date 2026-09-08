using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Mappings
{
    public static class ReportMappings
    {
        public static ReportDto ToDto(this ReportModel report)
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

        public static List<ReportDto> ToDtoList(this IEnumerable<ReportModel> reports)
        {
            return reports.Select(r => r.ToDto()).ToList();
        }

        public static ReportCategoryDto ToDto(this ReportCategoryModel cat)
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

        public static List<ReportCategoryDto> ToDtoList(this IEnumerable<ReportCategoryModel> categories)
        {
            return categories.Select(c => c.ToDto()).ToList();
        }
    }
}
