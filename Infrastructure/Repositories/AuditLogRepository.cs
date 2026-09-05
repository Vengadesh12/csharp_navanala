using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;

        public AuditLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<AuditLog> Logs, int TotalEvents, int SuccessfulLogins, int PrivilegeChanges)> GetAuditLogsOverviewAsync(string? module, string? search)
        {
            var sql = new StringBuilder("""
                SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                FROM audit_logs
                WHERE deleted_flag = 1
            """);

            var parameters = new List<object>();
            int paramIndex = 0;

            if (!string.IsNullOrWhiteSpace(module) && module != "ALL")
            {
                sql.Append($" AND LOWER(module) = LOWER({{{paramIndex++}}})");
                parameters.Add(module.Trim());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                sql.Append($" AND (LOWER(action) LIKE {{{paramIndex}}} OR LOWER(details) LIKE {{{paramIndex}}} OR LOWER(performed_by) LIKE {{{paramIndex++}}})");
                parameters.Add(pattern);
            }

            sql.Append(" ORDER BY id DESC");

            var rawLogs = await _context.AuditLogs
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            var totalEvents = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM audit_logs
                WHERE deleted_flag = 1
            """).SingleOrDefaultAsync();

            var successfulLogins = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM audit_logs
                WHERE deleted_flag = 1 AND module = 'Auth'
            """).SingleOrDefaultAsync();

            var privilegeChanges = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM audit_logs
                WHERE deleted_flag = 1 AND (module = 'Permissions' OR module = 'Roles')
            """).SingleOrDefaultAsync();

            return (rawLogs, totalEvents, successfulLogins, privilegeChanges);
        }

        public async Task<AuditLog> CreateAuditLogAsync(string action, string module, string performedBy, string details, string ipAddress, string status)
        {
            var now = DateTime.UtcNow;
            var newId = _context.Database.SqlQueryRaw<int>("""
                INSERT INTO audit_logs (action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {6}, 1)
                RETURNING id AS "Value"
            """, action, module, performedBy, details, ipAddress, status, now)
            .AsEnumerable()
            .Single();

            var log = await _context.AuditLogs
                .FromSqlRaw("""
                    SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                    FROM audit_logs
                    WHERE id = {0} AND deleted_flag = 1
                """, newId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return log!;
        }

        public async Task<bool> SoftDeleteAuditLogAsync(int id)
        {
            var now = DateTime.UtcNow;
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE audit_logs
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """, now, id);

            return rowsAffected > 0;
        }

        public async Task AddAuditLogAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetRecentAuditLogsAsync(int count)
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

        public async Task<List<AuditLog>> GetAuditLogsInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .FromSqlRaw("""
                    SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                    FROM audit_logs
                    WHERE deleted_flag = 1 AND created_at >= {0} AND created_at <= {1}
                    ORDER BY created_at ASC
                """, startDate, endDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
