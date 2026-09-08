using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Mappings
{
    public static class PermissionMappings
    {
        public static PermissionDto ToDto(this PermissionModel entity, bool isAssigned = false)
        {
            return new PermissionDto
            {
                PermissionKey = entity.PermissionKey,
                Name = entity.Name,
                Description = entity.Description,
                IsAssigned = isAssigned ? 1 : 0
            };
        }

        public static List<PermissionDto> ToDtoList(this IEnumerable<PermissionModel> entities, HashSet<string>? assignedKeys = null)
        {
            return entities.Select(e => e.ToDto(assignedKeys != null && assignedKeys.Contains(e.PermissionKey))).ToList();
        }
    }
}
