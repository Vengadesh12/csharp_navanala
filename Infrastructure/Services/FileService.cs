using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MyBackend.Application.Interfaces;

namespace MyBackend.Infrastructure.Services
{
    /// <summary>
    /// Implements disk-based file persistence and upload management for user profiles.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly string _uploadsBasePath;

        public FileService()
        {
            _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        }

        public async Task<string> SaveProfileImageAsync(Stream stream, string originalFileName, int userId, CancellationToken cancellationToken = default)
        {
            var profilesDir = Path.Combine(_uploadsBasePath, "profiles");
            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var uniqueFileName = $"user_{userId}_{Guid.NewGuid():N}{extension}";
            var destinationFilePath = Path.Combine(profilesDir, uniqueFileName);

            await using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }

            return $"/uploads/profiles/{uniqueFileName}";
        }

        public Task<bool> DeleteFileAsync(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return Task.FromResult(false);

            try
            {
                var cleanPath = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), cleanPath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
            }
            catch
            {
                // Fallback gracefully on disk deletion errors
            }

            return Task.FromResult(false);
        }
    }
}
