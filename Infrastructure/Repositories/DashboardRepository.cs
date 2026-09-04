using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetPermissionsCountAsync()
        {
            return await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM permissions
                WHERE "DeletedFlag" = 1
            """).SingleOrDefaultAsync();
        }

        public async Task<int> GetActiveSessionsCountAsync()
        {
            return await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM user_sessions
                WHERE deleted_flag = 1 AND is_active = true AND logout_time IS NULL
            """).SingleOrDefaultAsync();
        }

        public async Task<List<AuditLog>> GetDashboardRecentAuditLogsAsync(int count)
        {
            return await _context.AuditLogs
                .FromSqlRaw("""
                    SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                    FROM audit_logs
                    WHERE deleted_flag = 1
                    ORDER BY created_at DESC
                    LIMIT {0}
                """, count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsSinceDateAsync(DateTime startDate)
        {
            return await _context.AuditLogs
                .FromSqlRaw("""
                    SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                    FROM audit_logs
                    WHERE deleted_flag = 1 AND created_at >= {0}
                    ORDER BY created_at ASC
                """, startDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<UserSession>> GetAllActiveSessionsForDashboardAsync()
        {
            return await _context.UserSessions
                .FromSqlRaw("""
                    SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                    FROM user_sessions
                    WHERE deleted_flag = 1
                    ORDER BY login_time DESC
                """)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
