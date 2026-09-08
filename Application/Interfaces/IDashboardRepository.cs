using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetPermissionsCountAsync();

        Task<int> GetActiveSessionsCountAsync();

        Task<List<AuditLogModel>> GetDashboardRecentAuditLogsAsync(int count);

        Task<List<AuditLogModel>> GetAuditLogsSinceDateAsync(DateTime startDate);

        Task<List<UserSessionModel>> GetAllActiveSessionsForDashboardAsync();
    }
}
