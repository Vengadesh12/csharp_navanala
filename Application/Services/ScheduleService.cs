using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;

        public ScheduleService(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task<SchedulesOverviewResponse> GetSchedulesAsync(string? eventType, string? search)
        {
            var (rawSchedules, upcomingReviews, dueThisWeek, totalUsers, activeSessions) =
                await _scheduleRepository.GetSchedulesOverviewDataAsync(eventType, search);

            var eventTypes = await GetEventTypesAsync();

            var teamAvailability = totalUsers > 0
                ? $"{Math.Max(0, (int)Math.Round((double)(totalUsers - activeSessions) / totalUsers * 100))}%"
                : "100%";

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
            var (types, activeSchedules) = await _scheduleRepository.GetEventTypesWithCountsAsync();

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

            var existing = await _scheduleRepository.GetEventTypeByNameAsync(trimmedName);
            if (existing != null)
            {
                if (existing.DeletedFlag == 1)
                {
                    throw new BadRequestException($"Event type '{trimmedName}' already exists.");
                }

                await _scheduleRepository.UpdateEventTypeAsync(existing.Id, desc, color, icon);
                var eventCount = await _scheduleRepository.GetActiveEventCountForTypeAsync(existing.Name);

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

            var newId = await _scheduleRepository.CreateEventTypeAsync(trimmedName, desc, color, icon, createdBy);

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
            return await _scheduleRepository.SoftDeleteEventTypeAsync(id);
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

            var newId = await _scheduleRepository.CreateScheduleAsync(title, description, eventType, eventDate, startTime, endTime, location, organizer, status, priority, attendeesCount);
            var schedule = await _scheduleRepository.GetScheduleByIdAsync(newId);

            return schedule!.ToDto();
        }

        public async Task<ScheduleEventDto?> UpdateScheduleAsync(int id, UpdateScheduleRequest request)
        {
            var attendeesCount = Math.Max(1, request.AttendeesCount);
            var updated = await _scheduleRepository.UpdateScheduleAsync(
                id,
                request.Title.Trim(),
                request.Description.Trim(),
                request.EventType.Trim(),
                request.EventDate.Trim(),
                request.StartTime.Trim(),
                request.EndTime.Trim(),
                request.Location.Trim(),
                request.Organizer.Trim(),
                request.Status.Trim(),
                request.Priority.Trim(),
                attendeesCount);

            if (!updated) return null;

            var schedule = await _scheduleRepository.GetScheduleByIdAsync(id);
            return schedule?.ToDto();
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            return await _scheduleRepository.SoftDeleteScheduleAsync(id);
        }
    }
}
