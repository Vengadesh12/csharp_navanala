using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;

namespace MyBackend.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IApplicationDbContext _context;

        public AuditLogService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditLogOverviewResponse> GetAuditLogsAsync(string? module, string? search)
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
            var now = DateTime.UtcNow;

            var newId = await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO audit_logs (action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {6}, 1)
                RETURNING id AS "Value"
            """, action, module, performedBy, details, ip, status, now).SingleAsync();

            var log = await _context.AuditLogs
                .FromSqlRaw("""
                    SELECT id, action, module, performed_by, details, ip_address, status, created_at, updated_at, deleted_flag
                    FROM audit_logs
                    WHERE id = {0} AND deleted_flag = 1
                """, newId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return log!.ToDto();
        }

        public async Task<bool> DeleteAuditLogAsync(int id)
        {
            var now = DateTime.UtcNow;
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE audit_logs
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """, now, id);

            return rowsAffected > 0;
        }
    }
}
