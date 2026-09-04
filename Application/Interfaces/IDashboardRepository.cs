using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetPermissionsCountAsync();

        Task<int> GetActiveSessionsCountAsync();

        Task<List<AuditLog>> GetDashboardRecentAuditLogsAsync(int count);

        Task<List<AuditLog>> GetAuditLogsSinceDateAsync(DateTime startDate);

        Task<List<UserSession>> GetAllActiveSessionsForDashboardAsync();
    }
}
