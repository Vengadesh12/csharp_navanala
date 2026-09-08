using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Mappings
{
    public static class RoleMappings
    {
        public static RoleDto ToDto(this RoleModel entity)
        {
            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? string.Empty,
                DeletedFlag = entity.DeletedFlag
            };
        }

        public static List<RoleDto> ToDtoList(this IEnumerable<RoleModel> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
