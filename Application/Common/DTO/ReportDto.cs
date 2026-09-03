using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MyBackend.Application.Common.DTO;

public class ReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string Category { get; set; } = "Compliance";
    public string Format { get; set; } = "PDF";
    public string CreatedBy { get; set; } = "System Admin";
    public string Status { get; set; } = "Generated";
    public string FileSize { get; set; } = "1.2 MB";
    public string? FileName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int DeletedFlag { get; set; } = 1;
}

public class CreateReportRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string Category { get; set; } = "Compliance";
    public string Format { get; set; } = "PDF";
    public string? FileName { get; set; }
    public IFormFile? File { get; set; }
}

public class UpdateReportRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string Category { get; set; } = "Compliance";
    public string Format { get; set; } = "PDF";
    public string Status { get; set; } = "Generated";
    public string? FileName { get; set; }
    public IFormFile? File { get; set; }
}

public class ReportsOverviewResponse
{
    public int ReportsGenerated { get; set; }
    public int ExportsReady { get; set; }
    public string RoleCoverage { get; set; } = "100%";
    public List<ReportDto> Reports { get; set; } = [];
    public List<ReportCategoryDto> Categories { get; set; } = [];
}

public class ReportDownloadResult
{
    public byte[] FileBytes { get; set; } = [];
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = "report.txt";
}

public class ReportCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public int DeletedFlag { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateReportCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}
