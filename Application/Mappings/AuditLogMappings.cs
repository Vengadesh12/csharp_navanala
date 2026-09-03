using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class AuditLogMappings
    {
        public static AuditLogDto ToDto(this AuditLog log)
        {
            return new AuditLogDto
            {
                Id = log.Id,
                Action = log.Action ?? string.Empty,
                Module = log.Module ?? string.Empty,
                PerformedBy = log.PerformedBy ?? string.Empty,
                Details = log.Details ?? string.Empty,
                IpAddress = log.IpAddress ?? string.Empty,
                Status = log.Status ?? "Success",
                CreatedAt = log.CreatedAt,
                DeletedFlag = log.DeletedFlag
            };
        }

        public static List<AuditLogDto> ToDtoList(this IEnumerable<AuditLog> logs)
        {
            return logs.Select(l => l.ToDto()).ToList();
        }
    }
}
