using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
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

        public async Task<(List<AuditLogModel> Logs, int TotalEvents, int SuccessfulLogins, int PrivilegeChanges)> GetAuditLogsOverviewAsync(string? module, string? search)
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

            var totalEventsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM audit_logs
                WHERE deleted_flag = 1
            """);
            var totalEvents = await _context.Database.SqlQueryRaw<int>(totalEventsSql.ToString()).SingleOrDefaultAsync();

            var successfulLoginsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM audit_logs
                WHERE deleted_flag = 1 AND module = 'Auth'
            """);
            var successfulLogins = await _context.Database.SqlQueryRaw<int>(successfulLoginsSql.ToString()).SingleOrDefaultAsync();

            var privilegeChangesSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM audit_logs
                WHERE deleted_flag = 1 AND (module = 'Permissions' OR module = 'Roles')
            """);
            var privilegeChanges = await _context.Database.SqlQueryRaw<int>(privilegeChangesSql.ToString()).SingleOrDefaultAsync();

            return (rawLogs, totalEvents, successfulLogins, privilegeChanges);
        }

        public async Task<AuditLogModel> CreateAuditLogAsync(string action, string module, string performedBy, string details, string ipAddress, string status)
        {
            var now = DateTime.UtcNow;
            var insertSql = new StringBuilder("""
                INSERT INTO audit_logs (action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {6}, 1)
                RETURNING id AS "Value"
            """);

            var newId = _context.Database.SqlQueryRaw<int>(
                insertSql.ToString(),
                action, module, performedBy, details, ipAddress, status, now)
            .AsEnumerable()
            .Single();

            var getSql = new StringBuilder("""
                SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                FROM audit_logs
                WHERE id = {0} AND deleted_flag = 1
            """);

            var log = await _context.AuditLogs
                .FromSqlRaw(getSql.ToString(), newId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return log!;
        }

        public async Task<bool> SoftDeleteAuditLogAsync(int id)
        {
            var now = DateTime.UtcNow;
            var updateSql = new StringBuilder("""
                UPDATE audit_logs
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """);

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(updateSql.ToString(), now, id);
            return rowsAffected > 0;
        }

        public async Task AddAuditLogAsync(AuditLogModel log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLogModel>> GetRecentAuditLogsAsync(int count)
        {
            var sql = new StringBuilder("""
                SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                FROM audit_logs
                WHERE deleted_flag = 1
                ORDER BY created_at DESC
                LIMIT {0}
            """);

            return await _context.AuditLogs
                .FromSqlRaw(sql.ToString(), count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<AuditLogModel>> GetAuditLogsInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sql = new StringBuilder("""
                SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                FROM audit_logs
                WHERE deleted_flag = 1 AND created_at >= {0} AND created_at <= {1}
                ORDER BY created_at ASC
            """);

            return await _context.AuditLogs
                .FromSqlRaw(sql.ToString(), startDate, endDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
