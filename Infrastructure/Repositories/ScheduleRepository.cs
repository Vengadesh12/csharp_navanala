using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly AppDbContext _context;

        public ScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<ScheduleEvent> Schedules, int UpcomingReviews, int DueThisWeek, int TotalUsers, int ActiveSessions)> GetSchedulesOverviewDataAsync(string? eventType, string? search)
        {
            var sql = new StringBuilder("""
                SELECT id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, updated_at, deleted_flag
                FROM schedules
                WHERE deleted_flag = 1
            """);

            var parameters = new List<object>();
            int paramIndex = 0;

            if (!string.IsNullOrWhiteSpace(eventType) && eventType != "ALL")
            {
                sql.Append($" AND LOWER(event_type) = LOWER({{{paramIndex++}}})");
                parameters.Add(eventType.Trim());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                sql.Append($" AND (LOWER(title) LIKE {{{paramIndex}}} OR LOWER(description) LIKE {{{paramIndex}}} OR LOWER(organizer) LIKE {{{paramIndex}}} OR LOWER(location) LIKE {{{paramIndex++}}})");
                parameters.Add(pattern);
            }

            sql.Append(" ORDER BY event_date ASC, start_time ASC");

            var rawSchedules = await _context.Schedules
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            var upcomingReviews = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM schedules
                WHERE deleted_flag = 1 AND status = 'Scheduled'
            """).SingleOrDefaultAsync();

            var dueThisWeek = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM schedules
                WHERE deleted_flag = 1 AND (priority = 'High' OR priority = 'Urgent')
            """).SingleOrDefaultAsync();

            var totalUsers = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM users
                WHERE "DeletedFlag" = 1
            """).SingleOrDefaultAsync();

            var activeSessions = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM user_sessions
                WHERE deleted_flag = 1 AND is_active = TRUE AND logout_time IS NULL
            """).SingleOrDefaultAsync();

            return (rawSchedules, upcomingReviews, dueThisWeek, totalUsers, activeSessions);
        }

        public async Task<(List<EventType> EventTypes, List<string> ActiveScheduleTypes)> GetEventTypesWithCountsAsync()
        {
            var types = await _context.EventTypes
                .FromSqlRaw("""
                    SELECT id, name, description, color, icon, created_at, updated_at, created_by, deleted_flag
                    FROM event_types
                    WHERE deleted_flag = 1
                    ORDER BY id ASC
                """)
                .AsNoTracking()
                .ToListAsync();

            var activeSchedules = await _context.Database.SqlQueryRaw<string>("""
                SELECT event_type AS "Value"
                FROM schedules
                WHERE deleted_flag = 1
            """).ToListAsync();

            return (types, activeSchedules);
        }

        public async Task<EventType?> GetEventTypeByNameAsync(string name)
        {
            return await _context.EventTypes
                .FromSqlRaw("""
                    SELECT id, name, description, color, icon, created_at, updated_at, created_by, deleted_flag
                    FROM event_types
                    WHERE LOWER(name) = LOWER({0})
                """, name.Trim())
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateEventTypeAsync(int id, string description, string color, string icon)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE event_types
                SET deleted_flag = 1, description = {0}, color = {1}, icon = {2}, updated_at = {3}
                WHERE id = {4}
            """, description, color, icon, now, id);

            return rows > 0;
        }

        public async Task<int> GetActiveEventCountForTypeAsync(string typeName)
        {
            return await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM schedules
                WHERE deleted_flag = 1 AND LOWER(event_type) = LOWER({0})
            """, typeName).SingleOrDefaultAsync();
        }

        public async Task<int> CreateEventTypeAsync(string name, string description, string color, string icon, string createdBy)
        {
            var now = DateTime.UtcNow;
            return await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO event_types (name, description, color, icon, created_at, updated_at, created_by, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {4}, {5}, 1)
                RETURNING id AS "Value"
            """, name, description, color, icon, now, createdBy).SingleAsync();
        }

        public async Task<bool> SoftDeleteEventTypeAsync(int id)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE event_types
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """, now, id);

            return rows > 0;
        }

        public async Task<int> CreateScheduleAsync(string title, string description, string eventType, string eventDate, string startTime, string endTime, string location, string organizer, string status, string priority, int attendeesCount)
        {
            var now = DateTime.UtcNow;
            return await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO schedules (title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, updated_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {11}, 1)
                RETURNING id AS "Value"
            """, title, description, eventType, eventDate, startTime, endTime, location, organizer, status, priority, attendeesCount, now).SingleAsync();
        }

        public async Task<ScheduleEvent?> GetScheduleByIdAsync(int id)
        {
            return await _context.Schedules
                .FromSqlRaw("""
                    SELECT id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, updated_at, deleted_flag
                    FROM schedules
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateScheduleAsync(int id, string title, string description, string eventType, string eventDate, string startTime, string endTime, string location, string organizer, string status, string priority, int attendeesCount)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE schedules
                SET title = {0}, description = {1}, event_type = {2}, event_date = {3}, start_time = {4}, end_time = {5}, location = {6}, organizer = {7}, status = {8}, priority = {9}, attendees_count = {10}, updated_at = {11}
                WHERE id = {12} AND deleted_flag = 1
            """, title, description, eventType, eventDate, startTime, endTime, location, organizer, status, priority, attendeesCount, now, id);

            return rows > 0;
        }

        public async Task<bool> SoftDeleteScheduleAsync(int id)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE schedules
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """, now, id);

            return rows > 0;
        }
    }
}
