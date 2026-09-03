using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<(List<AuditLog> Logs, int TotalEvents, int SuccessfulLogins, int PrivilegeChanges)> GetAuditLogsOverviewAsync(string? module, string? search);

        Task<AuditLog> CreateAuditLogAsync(string action, string module, string performedBy, string details, string ipAddress, string status);

        Task<bool> SoftDeleteAuditLogAsync(int id);

        Task AddAuditLogAsync(AuditLog log);

        Task<List<AuditLog>> GetRecentAuditLogsAsync(int count);

        Task<List<AuditLog>> GetAuditLogsInDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
