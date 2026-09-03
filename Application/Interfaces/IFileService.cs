using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyBackend.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveProfileImageAsync(Stream stream, string originalFileName, int userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string? relativePath);
    }
}
