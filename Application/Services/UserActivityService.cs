using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;

        public UserActivityService(IUnitOfWork unitOfWork, IApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<UserActivitySummaryDto> GetSummaryAsync()
        {
            var stats = await _unitOfWork.Sessions.GetActivityStatsAsync();
            var activeSessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
            var recentSessions = await _unitOfWork.Sessions.GetAllRecentSessionsAsync(15);

            // Fetch user role map
            var roleMap = await GetUserRoleMapAsync();

            var activeDtos = activeSessions.ToItemDtoList(roleMap);
            var recentDtos = recentSessions.ToItemDtoList(roleMap);

            return new UserActivitySummaryDto
            {
                ActiveUsersCount = stats.ActiveCount,
                TotalLoginsToday = stats.TodayLogins,
                TotalLogoutsToday = stats.TodayLogouts,
                TotalSessionsRecorded = stats.TotalSessions,
                ActiveSessions = activeDtos,
                RecentActivities = recentDtos
            };
        }

        public async Task<PagedUserActivityResponse> GetPagedActivitiesAsync(UserActivityQueryParameters query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var (items, totalCount) = await _unitOfWork.Sessions.GetPagedSessionsAsync(query.Search, query.Status, page, pageSize);
            var roleMap = await GetUserRoleMapAsync();

            var dtos = items.ToItemDtoList(roleMap);

            return new PagedUserActivityResponse
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<UserSessionItemDto>> GetActiveUsersAsync()
        {
            var activeSessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
            var roleMap = await GetUserRoleMapAsync();

            return activeSessions.ToItemDtoList(roleMap);
        }

        public async Task<UserSessionDto?> GetSessionByIdAsync(int sessionId)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            return session?.ToDto();
        }

        public async Task<bool> TerminateSessionAsync(int sessionId, int adminUserId)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return false;

            var now = DateTime.UtcNow;
            session.EndSession(now);

            // Also mark any other open/active sessions for the same user as terminated
            if (session.UserId > 0)
            {
                var otherSessions = await _context.UserSessions
                    .Where(s => s.UserId == session.UserId && s.Id != session.Id && (s.IsActive || s.LogoutTime == null))
                    .ToListAsync();

                foreach (var other in otherSessions)
                {
                    other.EndSession(now);
                }
            }

            // Write audit log using Business Object factory
            try
            {
                var adminUser = await _context.Users.FindAsync(adminUserId);
                var adminName = adminUser?.Name ?? $"Admin #{adminUserId}";

                _context.AuditLogs.Add(AuditLog.CreateLog(
                    action: "Force Terminate Session",
                    module: "Auth",
                    performedBy: adminName,
                    details: $"Terminated active session #{sessionId} for user {session.UserName} ({session.Email})",
                    ipAddress: session.IpAddress,
                    status: "Success"
                ));
            }
            catch
            {
                // Ignore audit log error if write fails
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ForceLogoutUserAsync(int targetUserId, int adminUserId)
        {
            var user = await _context.Users.FindAsync(targetUserId);
            var now = DateTime.UtcNow;

            var activeSessions = await _context.UserSessions
                .Where(s => s.UserId == targetUserId && (s.IsActive || s.LogoutTime == null))
                .ToListAsync();

            if (activeSessions.Count > 0)
            {
                foreach (var session in activeSessions)
                {
                    session.EndSession(now);
                }
            }
            else if (user != null)
            {
                var emailSessions = await _context.UserSessions
                    .Where(s => s.Email.ToLower() == user.Email.ToLower() && (s.IsActive || s.LogoutTime == null))
                    .ToListAsync();

                foreach (var s in emailSessions)
                {
                    s.EndSession(now);
                }
            }

            // Record audit log using Business Object factory
            try
            {
                var adminUser = await _context.Users.FindAsync(adminUserId);
                var adminName = adminUser?.Name ?? $"Admin #{adminUserId}";

                _context.AuditLogs.Add(AuditLog.CreateLog(
                    action: "Force User Logout",
                    module: "Auth",
                    performedBy: adminName,
                    details: $"Terminated all active sessions for {user?.Name ?? $"User #{targetUserId}"}",
                    ipAddress: "127.0.0.1",
                    status: "Success"
                ));
            }
            catch
            {
                // Ignore audit log error
            }

            await _context.SaveChangesAsync();
            return Math.Max(1, activeSessions.Count);
        }

        private async Task<Dictionary<int, string>> GetUserRoleMapAsync()
        {
            try
            {
                var roles = await _context.Roles.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Name);
                var userRoles = await _context.Users.AsNoTracking()
                    .Where(u => u.DeletedFlag == 1 && u.RoleId != null)
                    .Select(u => new { u.Id, RoleId = u.RoleId!.Value })
                    .ToListAsync();

                var map = new Dictionary<int, string>();
                foreach (var ur in userRoles)
                {
                    if (roles.TryGetValue(ur.RoleId, out var roleName))
                    {
                        map[ur.Id] = roleName;
                    }
                }
                return map;
            }
            catch
            {
                return new Dictionary<int, string>();
            }
        }
    }
}
