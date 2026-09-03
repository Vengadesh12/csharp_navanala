using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// File storage service contract for handling profile uploads and disk management.
    /// </summary>
    public interface IFileService
    {
        Task<string> SaveProfileImageAsync(Stream stream, string originalFileName, int userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string? relativePath);
    }
}
