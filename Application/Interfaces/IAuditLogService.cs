using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<AuditLogOverviewResponse> GetAuditLogsAsync(string? module, string? search);
        Task<AuditLogDto> CreateAuditLogAsync(CreateAuditLogRequest request, string performedBy, string ipAddress);
        Task<bool> DeleteAuditLogAsync(int id);
    }
}
