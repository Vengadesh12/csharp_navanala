using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<(List<AuditLogModel> Logs, int TotalEvents, int SuccessfulLogins, int PrivilegeChanges)> GetAuditLogsOverviewAsync(string? module, string? search);

        Task<AuditLogModel> CreateAuditLogAsync(string action, string module, string performedBy, string details, string ipAddress, string status);

        Task<bool> SoftDeleteAuditLogAsync(int id);

        Task AddAuditLogAsync(AuditLogModel log);

        Task<List<AuditLogModel>> GetRecentAuditLogsAsync(int count);

        Task<List<AuditLogModel>> GetAuditLogsInDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
