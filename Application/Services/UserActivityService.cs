using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;

namespace MyBackend.Application.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserActivityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserActivitySummaryDto> GetSummaryAsync()
        {
            var stats = await _unitOfWork.Sessions.GetActivityStatsAsync();
            var activeSessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
            var recentSessions = await _unitOfWork.Sessions.GetAllRecentSessionsAsync(15);

            var roleMap = await _unitOfWork.Users.GetUserRoleMapAsync();

            var activeDtos = activeSessions.ToItemDtoList(roleMap);
            var recentDtos = recentSessions.ToItemDtoList(roleMap);

            var activeCount = Math.Max(stats.ActiveCount, activeDtos.Count);

            return new UserActivitySummaryDto
            {
                ActiveUsersCount = activeCount,
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
            var roleMap = await _unitOfWork.Users.GetUserRoleMapAsync();

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
            var roleMap = await _unitOfWork.Users.GetUserRoleMapAsync();

            return activeSessions.ToItemDtoList(roleMap);
        }

        public async Task<UserSessionDto?> GetSessionByIdAsync(int sessionId)
        {
            var session = await _unitOfWork.Sessions.GetSessionByIdAsync(sessionId);
            return session?.ToDto();
        }

        public async Task<bool> TerminateSessionAsync(int sessionId, int adminUserId)
        {
            return await _unitOfWork.Sessions.TerminateSessionWithAuditAsync(sessionId, adminUserId);
        }

        public async Task<int> ForceLogoutUserAsync(int targetUserId, int adminUserId)
        {
            return await _unitOfWork.Sessions.ForceLogoutUserWithAuditAsync(targetUserId, adminUserId);
        }
    }
}
