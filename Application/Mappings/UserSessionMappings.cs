using System;
using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class UserSessionMappings
    {
        public static UserSessionDto ToDto(this UserSession entity)
        {
            return new UserSessionDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Email = entity.Email,
                UserName = entity.UserName,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                LoginTime = entity.LoginTime,
                LogoutTime = entity.LogoutTime,
                SessionToken = entity.SessionToken,
                IsActive = entity.IsActive,
                DeletedFlag = entity.DeletedFlag
            };
        }

        public static List<UserSessionDto> ToDtoList(this IEnumerable<UserSession> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static UserSessionItemDto ToItemDto(this UserSession entity, Dictionary<int, string> roleMap)
        {
            var isCurrentlyActive = entity.IsActive && entity.LogoutTime == null;
            var roleName = roleMap.TryGetValue(entity.UserId, out var r) ? r : "Member";

            var (browser, os) = ParseUserAgent(entity.UserAgent);
            var durationFormatted = FormatDuration(entity.LoginTime, entity.LogoutTime, isCurrentlyActive);

            return new UserSessionItemDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Email = entity.Email,
                UserName = entity.UserName,
                RoleName = roleName,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                Browser = browser,
                Os = os,
                LoginTime = entity.LoginTime,
                LogoutTime = entity.LogoutTime,
                IsActive = isCurrentlyActive,
                DurationFormatted = durationFormatted,
                Status = isCurrentlyActive ? "Active" : "Completed"
            };
        }

        public static List<UserSessionItemDto> ToItemDtoList(this IEnumerable<UserSession> entities, Dictionary<int, string> roleMap)
        {
            return entities.Select(e => e.ToItemDto(roleMap)).ToList();
        }

        private static string FormatDuration(DateTime loginTime, DateTime? logoutTime, bool isActive)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime endUtc = logoutTime ?? nowUtc;
            TimeSpan span = endUtc - loginTime;

            if (span.TotalSeconds < 0)
            {
                DateTime nowLocal = DateTime.Now;
                DateTime endLocal = logoutTime ?? nowLocal;
                var altSpan = endLocal - loginTime;
                span = altSpan.TotalSeconds >= 0 ? altSpan : TimeSpan.Zero;
            }

            string timeStr;
            if (span.TotalHours >= 24)
                timeStr = $"{(int)span.TotalDays}d {span.Hours}h {span.Minutes}m {span.Seconds}s";
            else if (span.TotalHours >= 1)
                timeStr = $"{(int)span.TotalHours}h {span.Minutes}m {span.Seconds}s";
            else if (span.TotalMinutes >= 1)
                timeStr = $"{(int)span.TotalMinutes}m {span.Seconds}s";
            else
                timeStr = $"{(int)span.TotalSeconds}s";

            return isActive ? $"Active ({timeStr})" : timeStr;
        }

        private static (string Browser, string Os) ParseUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return ("Web Browser", "Desktop");

            var ua = userAgent;
            string browser = "Chrome";
            string os = "Windows";

            if (ua.Contains("Windows", StringComparison.OrdinalIgnoreCase)) os = "Windows";
            else if (ua.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) || ua.Contains("Mac OS", StringComparison.OrdinalIgnoreCase)) os = "macOS";
            else if (ua.Contains("Linux", StringComparison.OrdinalIgnoreCase)) os = "Linux";
            else if (ua.Contains("Android", StringComparison.OrdinalIgnoreCase)) os = "Android";
            else if (ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || ua.Contains("iPad", StringComparison.OrdinalIgnoreCase)) os = "iOS";

            if (ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) browser = "Microsoft Edge";
            else if (ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) browser = "Chrome";
            else if (ua.Contains("Firefox/", StringComparison.OrdinalIgnoreCase)) browser = "Firefox";
            else if (ua.Contains("Safari/", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) browser = "Safari";
            else if (ua.Contains("Postman", StringComparison.OrdinalIgnoreCase)) browser = "Postman API Client";

            return (browser, os);
        }
    }
}
