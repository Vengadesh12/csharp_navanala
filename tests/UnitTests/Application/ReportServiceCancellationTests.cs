using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Services;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Entities.Model;
using Xunit;

namespace MyBackend.UnitTests.Application
{
    public class ReportServiceCancellationTests : IDisposable
    {
        private readonly FakeReportRepository _reportRepository;
        private readonly FakeHostEnvironment _environment;
        private readonly ReportService _reportService;
        private readonly string _testDir;

        public ReportServiceCancellationTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "report_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);

            _reportRepository = new FakeReportRepository();
            _environment = new FakeHostEnvironment { ContentRootPath = _testDir };
            _reportService = new ReportService(_reportRepository, _environment);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDir))
                {
                    Directory.Delete(_testDir, true);
                }
            }
            catch
            {
                // Cleanup best-effort
            }
        }

        private class FakeHostEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = "Testing";
            public string ApplicationName { get; set; } = "UnitTests";
            public string ContentRootPath { get; set; } = string.Empty;
            public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
        }

        private class FakeFormFile : IFormFile
        {
            private readonly byte[] _content;
            public FakeFormFile(string fileName, long length)
            {
                FileName = fileName;
                Length = length;
                _content = length <= 1024 * 1024 ? new byte[(int)length] : new byte[1024]; // small memory footprint for tests
            }

            public string ContentType => "application/pdf";
            public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{FileName}\"";
            public IHeaderDictionary Headers => null!;
            public long Length { get; }
            public string Name => "file";
            public string FileName { get; }

            public void CopyTo(Stream target) => CopyToAsync(target, CancellationToken.None).GetAwaiter().GetResult();

            public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await target.WriteAsync(_content, 0, _content.Length, cancellationToken);
            }

            public Stream OpenReadStream() => new MemoryStream(_content);
        }

        private class FakeReportRepository : IReportRepository
        {
            public List<Report> Reports { get; } = new();
            public List<ReportCategory> Categories { get; } = new();
            public bool CreateRecordCalled { get; private set; }
            public bool UpdateRecordCalled { get; private set; }

            public Task<(List<Report> Reports, int TotalReports, int ReadyReports, int TotalUsers, int UsersWithRole, List<ReportCategory> Categories)> GetReportsOverviewDataAsync(string? category, string? search, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult((Reports, Reports.Count, Reports.Count, 1, 1, Categories));
            }

            public Task<List<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(new List<string> { "Compliance" });
            }

            public Task<Report?> GetReportByIdAsync(int id, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var r = Reports.Find(x => x.Id == id && x.DeletedFlag == 1);
                return Task.FromResult(r);
            }

            public Task<Report> AddReportAsync(Report report, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Reports.Add(report);
                return Task.FromResult(report);
            }

            public Task<Report> CreateReportRecordAsync(string title, string description, int? categoryId, string categoryName, string format, string creatorName, string fileSize, string? storedFileName, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                CreateRecordCalled = true;
                var report = new Report
                {
                    Id = Reports.Count + 1,
                    Title = title,
                    Description = description,
                    CategoryId = categoryId,
                    Category = categoryName,
                    Format = format,
                    CreatedBy = creatorName,
                    Status = "Ready",
                    FileSize = fileSize,
                    FileName = storedFileName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedFlag = 1
                };
                Reports.Add(report);
                return Task.FromResult(report);
            }

            public Task<Report?> UpdateReportRecordAsync(int id, string title, string description, int? categoryId, string categoryName, string format, string? status, string? newFileName, string? newFileSize, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                UpdateRecordCalled = true;
                var r = Reports.Find(x => x.Id == id && x.DeletedFlag == 1);
                if (r != null)
                {
                    r.Title = title;
                    r.Description = description;
                    r.CategoryId = categoryId;
                    r.Category = categoryName;
                    r.Format = format;
                    if (!string.IsNullOrEmpty(status)) r.Status = status;
                    if (!string.IsNullOrEmpty(newFileName)) r.FileName = newFileName;
                    if (!string.IsNullOrEmpty(newFileSize)) r.FileSize = newFileSize;
                    r.UpdatedAt = DateTime.UtcNow;
                }
                return Task.FromResult(r);
            }

            public Task UpdateReportAsync(Report report, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            public Task<bool> SoftDeleteReportAsync(int id, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var r = Reports.Find(x => x.Id == id);
                if (r != null) r.DeletedFlag = 0;
                return Task.FromResult(r != null);
            }

            public Task<List<ReportCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Categories);
            public Task<ReportCategory?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(Categories.Find(c => c.Id == id));
            public Task<ReportCategory?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(Categories.Find(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
            public Task<bool> CategoryExistsByNameAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(Categories.Exists(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
            public Task<ReportCategory> AddCategoryAsync(ReportCategory category, CancellationToken cancellationToken = default)
            {
                category.Id = Categories.Count + 1;
                Categories.Add(category);
                return Task.FromResult(category);
            }
            public Task<bool> SoftDeleteCategoryAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(true);
        }

        [Fact]
        public async Task CreateReportAsync_FileSizeExceeds15MB_ThrowsBadRequestException()
        {
            // Arrange: 16 MB file (16 * 1024 * 1024 = 16,777,216 bytes)
            var file = new FakeFormFile("sample_large.pdf", 16L * 1024 * 1024);
            var req = new CreateReportRequest
            {
                Title = "Large Report",
                Description = "Testing size limit",
                Category = "Compliance",
                Format = "PDF",
                File = file
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _reportService.CreateReportAsync(req, "Admin"));

            Assert.Contains("15 MB", ex.Message);
            Assert.False(_reportRepository.CreateRecordCalled);
        }

        [Fact]
        public async Task CreateReportAsync_FileSizeWithin15MB_Succeeds()
        {
            // Arrange: 14 MB file
            var file = new FakeFormFile("valid_report.pdf", 14L * 1024 * 1024);
            var req = new CreateReportRequest
            {
                Title = "Valid Report",
                Description = "Under 15MB limit",
                Category = "Compliance",
                Format = "PDF",
                File = file
            };

            // Act
            var result = await _reportService.CreateReportAsync(req, "Admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Valid Report", result.Title);
            Assert.True(_reportRepository.CreateRecordCalled);
            Assert.NotNull(result.FileName);

            // Verify file exists on disk
            var reportDir = Path.Combine(_testDir, "report");
            Assert.True(File.Exists(Path.Combine(reportDir, result.FileName)));
        }

        [Fact]
        public async Task CreateReportAsync_WhenCancelled_ThrowsOperationCanceledException_AndCleansUpFile()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Pre-cancel token to simulate user navigating away immediately

            var file = new FakeFormFile("cancelled_report.pdf", 2L * 1024 * 1024);
            var req = new CreateReportRequest
            {
                Title = "Cancelled Report",
                Description = "User redirects to another menu",
                Category = "Compliance",
                Format = "PDF",
                File = file
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                _reportService.CreateReportAsync(req, "Admin", cts.Token));

            // Verify repository record was NOT created
            Assert.False(_reportRepository.CreateRecordCalled);

            // Verify report directory has NO leftover file
            var reportDir = Path.Combine(_testDir, "report");
            if (Directory.Exists(reportDir))
            {
                var files = Directory.GetFiles(reportDir);
                Assert.Empty(files);
            }
        }

        [Fact]
        public async Task UpdateReportAsync_FileSizeExceeds15MB_ThrowsBadRequestException()
        {
            // Arrange: Create existing report
            var initialReq = new CreateReportRequest
            {
                Title = "Existing Report",
                Description = "Initial description",
                Category = "Compliance",
                Format = "PDF"
            };
            var existing = await _reportService.CreateReportAsync(initialReq, "Admin");

            // Act: Update with 16MB file
            var file = new FakeFormFile("large_update.pdf", 16L * 1024 * 1024);
            var updateReq = new UpdateReportRequest
            {
                Title = "Updated Title",
                Description = "Updated Description",
                File = file
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _reportService.UpdateReportAsync(existing.Id, updateReq));

            Assert.Contains("15 MB", ex.Message);
            Assert.False(_reportRepository.UpdateRecordCalled);
        }

        [Fact]
        public async Task UpdateReportAsync_WhenCancelled_ThrowsOperationCanceledException_AndDoesNotModifyRecordOrSaveFile()
        {
            // Arrange: Create existing report first
            var initialFile = new FakeFormFile("initial.pdf", 1024);
            var initialReq = new CreateReportRequest
            {
                Title = "Existing Report",
                Description = "Initial description",
                Category = "Compliance",
                Format = "PDF",
                File = initialFile
            };
            var existing = await _reportService.CreateReportAsync(initialReq, "Admin");
            var originalFileName = existing.FileName;

            // Act: User updates with a new file but redirects (token cancelled)
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var newFile = new FakeFormFile("new_file.pdf", 5L * 1024 * 1024);
            var updateReq = new UpdateReportRequest
            {
                Title = "Attempted Update",
                Description = "Cancelled update",
                File = newFile
            };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                _reportService.UpdateReportAsync(existing.Id, updateReq, cts.Token));

            // Assert: Update record was NOT called
            Assert.False(_reportRepository.UpdateRecordCalled);

            // Assert: Existing file remains intact, but no new file exists
            var reportDir = Path.Combine(_testDir, "report");
            Assert.True(File.Exists(Path.Combine(reportDir, originalFileName!)));

            var allFiles = Directory.GetFiles(reportDir);
            Assert.Single(allFiles); // Only the original file exists
        }

        [Fact]
        public async Task UpdateReportAsync_WhenNotCancelled_UpdatesRecordAndReplacesFileCorrectly()
        {
            // Arrange
            var initialFile = new FakeFormFile("old.pdf", 1024);
            var initialReq = new CreateReportRequest
            {
                Title = "Existing Report",
                Description = "Old description",
                Category = "Compliance",
                Format = "PDF",
                File = initialFile
            };
            var existing = await _reportService.CreateReportAsync(initialReq, "Admin");
            var oldFileName = existing.FileName!;
            var reportDir = Path.Combine(_testDir, "report");
            Assert.True(File.Exists(Path.Combine(reportDir, oldFileName)));

            // Act: Perform valid update with a new file
            var newFile = new FakeFormFile("new.pdf", 4L * 1024 * 1024);
            var updateReq = new UpdateReportRequest
            {
                Title = "Updated Title",
                Description = "New description",
                Category = "Compliance",
                Format = "PDF",
                File = newFile
            };

            var updated = await _reportService.UpdateReportAsync(existing.Id, updateReq, CancellationToken.None);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("Updated Title", updated.Title);
            Assert.True(_reportRepository.UpdateRecordCalled);
            Assert.NotEqual(oldFileName, updated.FileName);

            // Verify old file was deleted and new file exists
            Assert.False(File.Exists(Path.Combine(reportDir, oldFileName)));
            Assert.True(File.Exists(Path.Combine(reportDir, updated.FileName!)));
        }
    }
}
