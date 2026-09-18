using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Configuration;
using MyBackend.Domain.Models;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class UserSessionRepository : Repository<UserSessionModel>, IUserSessionRepository
    {
        public UserSessionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<UserSessionModel> RecordLoginAsync(int userId, string email, string userName, string ipAddress, string? userAgent = null, string? sessionToken = null)
        {
            var now = DateTime.UtcNow;

            // Clean up: terminate any prior open active sessions for this user
            var cleanupSql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE deleted_flag = 1 AND is_active = true AND logout_time IS NULL AND user_id = {0}
            """);

            var sessionsToDeactivate = await _context.UserSessions
                .FromSqlRaw(cleanupSql.ToString(), userId)
                .ToListAsync();

            foreach (var oldSession in sessionsToDeactivate)
            {
                oldSession.IsActive = false;
                oldSession.LogoutTime = oldSession.UpdatedAt ?? now;
                oldSession.UpdatedAt = now;
            }

            var session = new UserSessionModel
            {
                UserId = userId,
                Email = email,
                UserName = userName,
                IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress.Trim(),
                UserAgent = userAgent,
                LoginTime = now,
                LogoutTime = null,
                SessionToken = sessionToken,
                IsActive = true,
                DeletedFlag = 1
            };

            await _context.UserSessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<bool> RecordLogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null)
        {
            var now = DateTime.UtcNow;
            var clientIp = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress.Trim();

            // 1. If specific session token is provided, prioritize terminating that exact session
            List<UserSessionModel> activeSessions = new();
            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                var tokenSql = new StringBuilder("""
                    SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                    FROM user_sessions
                    WHERE deleted_flag = 1 AND session_token = {0} AND (logout_time IS NULL OR is_active = true)
                """);

                activeSessions = await _context.UserSessions
                    .FromSqlRaw(tokenSql.ToString(), sessionToken)
                    .ToListAsync();
            }

            // 2. If no session was found by token, look up active sessions by userId or email
            if (activeSessions.Count == 0)
            {
                var sessionSql = new StringBuilder("""
                    SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                    FROM user_sessions
                    WHERE deleted_flag = 1 AND (logout_time IS NULL OR is_active = true)
                """);

                if (userId > 0)
                {
                    sessionSql.Append(" AND user_id = {0}");
                    activeSessions = await _context.UserSessions
                        .FromSqlRaw(sessionSql.ToString(), userId)
                        .ToListAsync();
                }
                else if (!string.IsNullOrWhiteSpace(email))
                {
                    sessionSql.Append(" AND LOWER(email) = LOWER({0})");
                    activeSessions = await _context.UserSessions
                        .FromSqlRaw(sessionSql.ToString(), email.Trim())
                        .ToListAsync();
                }
            }

            if (activeSessions.Count > 0)
            {
                foreach (var session in activeSessions)
                {
                    session.LogoutTime = now;
                    session.IsActive = false;
                    if (!string.IsNullOrWhiteSpace(ipAddress))
                    {
                        session.IpAddress = clientIp;
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }

            // Fallback: If no open session row existed, create a completed session row so history is preserved
            UserModel? user = null;
            if (userId > 0)
            {
                var userSql = new StringBuilder("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                    FROM users
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """);

                user = await _context.Users
                    .FromSqlRaw(userSql.ToString(), userId)
                    .FirstOrDefaultAsync();
            }
            if (user == null && !string.IsNullOrWhiteSpace(email))
            {
                var userEmailSql = new StringBuilder("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                    FROM users
                    WHERE LOWER("Email") = LOWER({0}) AND "DeletedFlag" = 1
                """);

                user = await _context.Users
                    .FromSqlRaw(userEmailSql.ToString(), email.Trim())
                    .FirstOrDefaultAsync();
            }

            if (user is not null)
            {
                var auditSession = new UserSessionModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.Name,
                    IpAddress = clientIp,
                    LoginTime = now.AddMinutes(-5),
                    LogoutTime = now,
                    SessionToken = sessionToken,
                    IsActive = false,
                    DeletedFlag = 1
                };
                await _context.UserSessions.AddAsync(auditSession);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<UserSessionModel>> GetUserSessionsAsync(int userId, int limit = 50)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE user_id = {0} AND deleted_flag = 1
                ORDER BY login_time DESC
                LIMIT {1}
            """);

            return await _context.UserSessions
                .FromSqlRaw(sql.ToString(), userId, limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<UserSessionModel>> GetAllRecentSessionsAsync(int limit = 100)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE deleted_flag = 1
                ORDER BY login_time DESC
                LIMIT {0}
            """);

            return await _context.UserSessions
                .FromSqlRaw(sql.ToString(), limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<UserSessionModel>> GetActiveSessionsAsync()
        {
            var maxMinutes = Config.SessionTimeoutMinutes > 0 ? Config.SessionTimeoutMinutes : 300;
            var cutoff = DateTime.UtcNow.AddMinutes(-maxMinutes);

            // Auto-deactivate any orphaned active sessions that exceeded the 5-hour limit
            try
            {
                var expiredSql = new StringBuilder("""
                    SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                    FROM user_sessions
                    WHERE deleted_flag = 1 AND is_active = true AND logout_time IS NULL AND login_time < {0}
                """);

                var expiredSessions = await _context.UserSessions
                    .FromSqlRaw(expiredSql.ToString(), cutoff)
                    .ToListAsync();

                if (expiredSessions.Count > 0)
                {
                    foreach (var exp in expiredSessions)
                    {
                        exp.IsActive = false;
                        exp.LogoutTime = exp.LoginTime.AddMinutes(maxMinutes);
                        exp.UpdatedAt = DateTime.UtcNow;
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                // Non-blocking in case of concurrent updates
            }

            var activeSql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE deleted_flag = 1 AND is_active = true AND logout_time IS NULL AND login_time >= {0}
                ORDER BY login_time DESC
            """);

            return await _context.UserSessions
                .FromSqlRaw(activeSql.ToString(), cutoff)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<UserSessionModel> Items, int TotalCount)> GetPagedSessionsAsync(string? search, string? status, int page, int pageSize)
        {
            var countSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM user_sessions WHERE deleted_flag = 1");
            var dataSql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE deleted_flag = 1
            """);

            var parameters = new List<object>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchParam = $"%{search.Trim().ToLower()}%";
                var idx = parameters.Count;
                var searchClause = $" AND (LOWER(user_name) LIKE {{{idx}}} OR LOWER(email) LIKE {{{idx}}} OR LOWER(ip_address) LIKE {{{idx}}})";
                countSql.Append(searchClause);
                dataSql.Append(searchClause);
                parameters.Add(searchParam);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusLower = status.Trim().ToLower();
                if (statusLower == "active")
                {
                    var statusClause = " AND is_active = true AND logout_time IS NULL";
                    countSql.Append(statusClause);
                    dataSql.Append(statusClause);
                }
                else if (statusLower == "completed" || statusLower == "inactive" || statusLower == "loggedout")
                {
                    var statusClause = " AND (is_active = false OR logout_time IS NOT NULL)";
                    countSql.Append(statusClause);
                    dataSql.Append(statusClause);
                }
            }

            var totalCount = await _context.Database
                .SqlQueryRaw<int>(countSql.ToString(), parameters.ToArray())
                .SingleOrDefaultAsync();

            var skip = Math.Max(0, (page - 1) * pageSize);
            var limitIdx = parameters.Count;
            var offsetIdx = parameters.Count + 1;

            dataSql.Append($" ORDER BY login_time DESC LIMIT {{{limitIdx}}} OFFSET {{{offsetIdx}}}");
            parameters.Add(pageSize);
            parameters.Add(skip);

            var items = await _context.UserSessions
                .FromSqlRaw(dataSql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> TerminateSessionAsync(int sessionId)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE id = {0} AND deleted_flag = 1
            """);

            var session = await _context.UserSessions
                .FromSqlRaw(sql.ToString(), sessionId)
                .FirstOrDefaultAsync();

            if (session == null) return false;

            session.IsActive = false;
            session.LogoutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> TerminateAllUserSessionsAsync(int userId)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE user_id = {0} AND deleted_flag = 1 AND (is_active = true OR logout_time IS NULL)
            """);

            var activeSessions = await _context.UserSessions
                .FromSqlRaw(sql.ToString(), userId)
                .ToListAsync();

            if (activeSessions.Count == 0) return 0;

            var now = DateTime.UtcNow;
            foreach (var session in activeSessions)
            {
                session.IsActive = false;
                session.LogoutTime = now;
            }

            await _context.SaveChangesAsync();
            return activeSessions.Count;
        }

        public async Task<(int ActiveCount, int TodayLogins, int TodayLogouts, int TotalSessions)> GetActivityStatsAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var maxMinutes = Config.SessionTimeoutMinutes > 0 ? Config.SessionTimeoutMinutes : 300;
            var cutoff = DateTime.UtcNow.AddMinutes(-maxMinutes);

            var activeSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM user_sessions
                WHERE deleted_flag = 1 AND is_active = true AND logout_time IS NULL AND login_time >= {0}
            """);
            var activeCount = await _context.Database.SqlQueryRaw<int>(activeSql.ToString(), cutoff).SingleOrDefaultAsync();

            var todayLoginsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM user_sessions
                WHERE deleted_flag = 1 AND login_time >= {0}
            """);
            var todayLogins = await _context.Database.SqlQueryRaw<int>(todayLoginsSql.ToString(), todayUtc).SingleOrDefaultAsync();

            var todayLogoutsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM user_sessions
                WHERE deleted_flag = 1 AND logout_time IS NOT NULL AND logout_time >= {0}
            """);
            var todayLogouts = await _context.Database.SqlQueryRaw<int>(todayLogoutsSql.ToString(), todayUtc).SingleOrDefaultAsync();

            var totalSessionsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM user_sessions
                WHERE deleted_flag = 1
            """);
            var totalSessions = await _context.Database.SqlQueryRaw<int>(totalSessionsSql.ToString()).SingleOrDefaultAsync();

            return (activeCount, todayLogins, todayLogouts, totalSessions);
        }

        public async Task<UserSessionModel?> GetSessionByIdAsync(int sessionId)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE id = {0} AND deleted_flag = 1
            """);

            return await _context.UserSessions
                .FromSqlRaw(sql.ToString(), sessionId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserSessionModel>> GetActiveSessionsForUserAsync(int userId, int? excludeSessionId = null)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE user_id = {0} AND (is_active = true OR logout_time IS NULL)
            """);

            if (excludeSessionId.HasValue)
            {
                sql.Append(" AND id != {1}");
                return await _context.UserSessions
                    .FromSqlRaw(sql.ToString(), userId, excludeSessionId.Value)
                    .ToListAsync();
            }

            return await _context.UserSessions
                .FromSqlRaw(sql.ToString(), userId)
                .ToListAsync();
        }

        public async Task<List<UserSessionModel>> GetActiveSessionsForEmailAsync(string email)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE LOWER(email) = LOWER({0}) AND (is_active = true OR logout_time IS NULL)
            """);

            return await _context.UserSessions
                .FromSqlRaw(sql.ToString(), email)
                .ToListAsync();
        }

        public async Task<UserSessionModel?> FindActiveSessionByTokenAsync(int userId, string token)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE user_id = {0} AND session_token = {1} AND deleted_flag = 1
                ORDER BY login_time DESC
                LIMIT 1
            """);

            return await _context.UserSessions
                .FromSqlRaw(sql.ToString(), userId, token)
                .FirstOrDefaultAsync();
        }

        public async Task TouchSessionAsync(int sessionId, string clientIp)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                FROM user_sessions
                WHERE id = {0}
            """);

            var session = await _context.UserSessions
                .FromSqlRaw(sql.ToString(), sessionId)
                .FirstOrDefaultAsync();

            if (session != null)
            {
                session.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(clientIp) && session.IpAddress != clientIp)
                {
                    session.IpAddress = clientIp;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetActiveSessionsCountAsync()
        {
            var sql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM user_sessions
                WHERE deleted_flag = 1 AND is_active = true AND logout_time IS NULL
            """);

            return await _context.Database.SqlQueryRaw<int>(sql.ToString()).SingleOrDefaultAsync();
        }

        public async Task<bool> TerminateSessionWithAuditAsync(int sessionId, int adminUserId)
        {
            var session = await GetSessionByIdAsync(sessionId);
            if (session == null) return false;

            var now = DateTime.UtcNow;
            session.IsActive = false;
            session.LogoutTime = now;
            session.UpdatedAt = now;

            if (session.UserId > 0)
            {
                var otherSessions = await GetActiveSessionsForUserAsync(session.UserId, session.Id);
                foreach (var other in otherSessions)
                {
                    other.IsActive = false;
                    other.LogoutTime = now;
                    other.UpdatedAt = now;
                }
            }

            try
            {
                var adminSql = new StringBuilder("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                    FROM users
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """);

                var adminUser = await _context.Users
                    .FromSqlRaw(adminSql.ToString(), adminUserId)
                    .FirstOrDefaultAsync();

                var adminName = adminUser?.Name ?? $"Admin #{adminUserId}";

                _context.AuditLogs.Add(new AuditLogModel
                {
                    Action = "Force Terminate Session",
                    Module = "Auth",
                    PerformedBy = adminName,
                    Details = $"Terminated active session #{sessionId} for user {session.UserName} ({session.Email})",
                    IpAddress = session.IpAddress,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedFlag = 1
                });
            }
            catch
            {
                // Ignore audit log error
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ForceLogoutUserWithAuditAsync(int targetUserId, int adminUserId)
        {
            var userSql = new StringBuilder("""
                SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                FROM users
                WHERE "Id" = {0} AND "DeletedFlag" = 1
            """);

            var user = await _context.Users
                .FromSqlRaw(userSql.ToString(), targetUserId)
                .FirstOrDefaultAsync();

            var now = DateTime.UtcNow;
            var activeSessions = await GetActiveSessionsForUserAsync(targetUserId);

            if (activeSessions.Count > 0)
            {
                foreach (var session in activeSessions)
                {
                    session.IsActive = false;
                    session.LogoutTime = now;
                    session.UpdatedAt = now;
                }
            }
            else if (user != null)
            {
                var emailSessions = await GetActiveSessionsForEmailAsync(user.Email);
                foreach (var s in emailSessions)
                {
                    s.IsActive = false;
                    s.LogoutTime = now;
                    s.UpdatedAt = now;
                }
            }

            try
            {
                var adminSql = new StringBuilder("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                    FROM users
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """);

                var adminUser = await _context.Users
                    .FromSqlRaw(adminSql.ToString(), adminUserId)
                    .FirstOrDefaultAsync();

                var adminName = adminUser?.Name ?? $"Admin #{adminUserId}";

                _context.AuditLogs.Add(new AuditLogModel
                {
                    Action = "Force UserModel Logout",
                    Module = "Auth",
                    PerformedBy = adminName,
                    Details = $"Terminated all active sessions for {user?.Name ?? $"UserModel #{targetUserId}"}",
                    IpAddress = "127.0.0.1",
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedFlag = 1
                });
            }
            catch
            {
                // Ignore audit log error
            }

            await _context.SaveChangesAsync();
            return Math.Max(1, activeSessions.Count);
        }

        public async Task AddSessionAsync(UserSessionModel session)
        {
            _context.UserSessions.Add(session);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Ignore concurrency collision if another request created it simultaneously
            }
        }
    }
}
