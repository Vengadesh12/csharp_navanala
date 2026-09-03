using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<AuditLogOverviewResponse> GetAuditLogsAsync(string? module, string? search)
        {
            var (rawLogs, totalEvents, successfulLogins, privilegeChanges) =
                await _auditLogRepository.GetAuditLogsOverviewAsync(module, search);

            return new AuditLogOverviewResponse
            {
                TotalEvents = totalEvents,
                SuccessfulLogins = successfulLogins,
                PrivilegeChanges = privilegeChanges,
                Logs = rawLogs.ToDtoList()
            };
        }

        public async Task<AuditLogDto> CreateAuditLogAsync(CreateAuditLogRequest request, string performedBy, string ipAddress)
        {
            var action = request.Action.Trim();
            var module = request.Module.Trim();
            var details = request.Details.Trim();
            var status = string.IsNullOrWhiteSpace(request.Status) ? "Success" : request.Status.Trim();
            var ip = string.IsNullOrWhiteSpace(request.IpAddress) ? ipAddress : request.IpAddress;

            var log = await _auditLogRepository.CreateAuditLogAsync(action, module, performedBy, details, ip, status);
            return log.ToDto();
        }

        public async Task<bool> DeleteAuditLogAsync(int id)
        {
            return await _auditLogRepository.SoftDeleteAuditLogAsync(id);
        }
    }
}
