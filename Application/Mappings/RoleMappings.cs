using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class RoleMappings
    {
        public static RoleDto ToDto(this Role entity)
        {
            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? string.Empty,
                DeletedFlag = entity.DeletedFlag
            };
        }

        public static List<RoleDto> ToDtoList(this IEnumerable<Role> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
