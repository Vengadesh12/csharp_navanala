using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;

namespace MyBackend.Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IApplicationDbContext _context;

        public ScheduleService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SchedulesOverviewResponse> GetSchedulesAsync(string? eventType, string? search)
        {
            var sql = new StringBuilder("""
                SELECT id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag
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

            var eventTypes = await GetEventTypesAsync();

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

            var teamAvailability = totalUsers > 0 ? $"{Math.Max(0, (int)Math.Round((double)(totalUsers - activeSessions) / totalUsers * 100))}%" : "100%";

            return new SchedulesOverviewResponse
            {
                UpcomingReviews = upcomingReviews,
                TeamAvailability = teamAvailability,
                DueThisWeek = dueThisWeek,
                Schedules = rawSchedules.ToDtoList(),
                EventTypes = eventTypes
            };
        }

        public async Task<List<EventTypeDto>> GetEventTypesAsync()
        {
            var types = await _context.EventTypes
                .FromSqlRaw("""
                    SELECT id, name, description, color, icon, created_at, created_by, deleted_flag
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

            return types.Select(t => new EventTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Color = string.IsNullOrWhiteSpace(t.Color) ? "#3b82f6" : t.Color,
                Icon = string.IsNullOrWhiteSpace(t.Icon) ? "Event" : t.Icon,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                EventCount = activeSchedules.Count(s => string.Equals(s, t.Name, StringComparison.OrdinalIgnoreCase))
            }).ToList();
        }

        public async Task<EventTypeDto> CreateEventTypeAsync(CreateEventTypeRequest request, string creatorName)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Event type name is required.");
            }

            var trimmedName = request.Name.Trim();
            var desc = request.Description?.Trim() ?? string.Empty;
            var color = string.IsNullOrWhiteSpace(request.Color) ? "#3b82f6" : request.Color.Trim();
            var icon = string.IsNullOrWhiteSpace(request.Icon) ? "Event" : request.Icon.Trim();
            var createdBy = string.IsNullOrWhiteSpace(creatorName) ? "System Admin" : creatorName;
            var now = DateTime.UtcNow;

            var existing = await _context.EventTypes
                .FromSqlRaw("""
                    SELECT id, name, description, color, icon, created_at, created_by, deleted_flag
                    FROM event_types
                    WHERE LOWER(name) = LOWER({0})
                """, trimmedName)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                if (existing.DeletedFlag == 1)
                {
                    throw new BadRequestException($"Event type '{trimmedName}' already exists.");
                }

                await _context.Database.ExecuteSqlRawAsync("""
                    UPDATE event_types
                    SET deleted_flag = 1, description = {0}, color = {1}, icon = {2}
                    WHERE id = {3}
                """, desc, color, icon, existing.Id);

                var eventCount = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM schedules
                    WHERE deleted_flag = 1 AND LOWER(event_type) = LOWER({0})
                """, existing.Name).SingleOrDefaultAsync();

                return new EventTypeDto
                {
                    Id = existing.Id,
                    Name = existing.Name,
                    Description = desc,
                    Color = color,
                    Icon = icon,
                    CreatedAt = existing.CreatedAt,
                    CreatedBy = existing.CreatedBy,
                    EventCount = eventCount
                };
            }

            var newId = await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO event_types (name, description, color, icon, created_at, created_by, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, 1)
                RETURNING id AS "Value"
            """, trimmedName, desc, color, icon, now, createdBy).SingleAsync();

            return new EventTypeDto
            {
                Id = newId,
                Name = trimmedName,
                Description = desc,
                Color = color,
                Icon = icon,
                CreatedAt = now,
                CreatedBy = createdBy,
                EventCount = 0
            };
        }

        public async Task<bool> DeleteEventTypeAsync(int id)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE event_types
                SET deleted_flag = 0
                WHERE id = {0} AND deleted_flag = 1
            """, id);

            return rowsAffected > 0;
        }

        public async Task<ScheduleEventDto> CreateScheduleAsync(CreateScheduleRequest request, string creatorName)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BadRequestException("Event title is required.");
            }

            var title = request.Title.Trim();
            var description = request.Description.Trim();
            var eventType = string.IsNullOrWhiteSpace(request.EventType) ? "Audit" : request.EventType.Trim();
            var eventDate = string.IsNullOrWhiteSpace(request.EventDate) ? DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd") : request.EventDate.Trim();
            var startTime = string.IsNullOrWhiteSpace(request.StartTime) ? "10:00 AM" : request.StartTime.Trim();
            var endTime = string.IsNullOrWhiteSpace(request.EndTime) ? "11:00 AM" : request.EndTime.Trim();
            var location = string.IsNullOrWhiteSpace(request.Location) ? "Virtual / Workspace" : request.Location.Trim();
            var organizer = string.IsNullOrWhiteSpace(request.Organizer) ? creatorName : request.Organizer.Trim();
            var status = string.IsNullOrWhiteSpace(request.Status) ? "Scheduled" : request.Status.Trim();
            var priority = string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority.Trim();
            var attendeesCount = Math.Max(1, request.AttendeesCount);
            var now = DateTime.UtcNow;

            var newId = await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO schedules (title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, 1)
                RETURNING id AS "Value"
            """, title, description, eventType, eventDate, startTime, endTime, location, organizer, status, priority, attendeesCount, now).SingleAsync();

            var schedule = await _context.Schedules
                .FromSqlRaw("""
                    SELECT id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag
                    FROM schedules
                    WHERE id = {0} AND deleted_flag = 1
                """, newId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return schedule!.ToDto();
        }

        public async Task<ScheduleEventDto?> UpdateScheduleAsync(int id, UpdateScheduleRequest request)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE schedules
                SET title = {0}, description = {1}, event_type = {2}, event_date = {3}, start_time = {4}, end_time = {5}, location = {6}, organizer = {7}, status = {8}, priority = {9}, attendees_count = {10}
                WHERE id = {11} AND deleted_flag = 1
            """, request.Title.Trim(), request.Description.Trim(), request.EventType.Trim(), request.EventDate.Trim(), request.StartTime.Trim(), request.EndTime.Trim(), request.Location.Trim(), request.Organizer.Trim(), request.Status.Trim(), request.Priority.Trim(), Math.Max(1, request.AttendeesCount), id);

            if (rowsAffected == 0) return null;

            var updated = await _context.Schedules
                .FromSqlRaw("""
                    SELECT id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag
                    FROM schedules
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return updated?.ToDto();
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE schedules
                SET deleted_flag = 0
                WHERE id = {0} AND deleted_flag = 1
            """, id);

            return rowsAffected > 0;
        }
    }
}
