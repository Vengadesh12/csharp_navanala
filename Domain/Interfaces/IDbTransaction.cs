using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyBackend.Domain.Interfaces
{
    /// <summary>
    /// Transaction abstraction decoupling Domain and Application from EF Core's IDbContextTransaction.
    /// </summary>
    public interface IDbTransaction : IDisposable, IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
