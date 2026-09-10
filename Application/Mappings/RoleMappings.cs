using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Mappings
{
    public static class RoleMappings
    {
        public static RoleDto ToDto(this RoleModel entity, string? parentRoleName = null)
        {
            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? string.Empty,
                ParentRoleId = entity.ParentRoleId,
                ParentRoleName = parentRoleName ?? entity.ParentRole?.Name,
                DeletedFlag = entity.DeletedFlag,
                CreatedAt = entity.CreatedAt
            };
        }

        public static List<RoleDto> ToDtoList(this IEnumerable<RoleModel> entities)
        {
            var list = entities.ToList();
            var roleMap = list.ToDictionary(r => r.Id, r => r.Name);
            return list.Select(e =>
            {
                string? parentName = null;
                if (e.ParentRoleId.HasValue && roleMap.TryGetValue(e.ParentRoleId.Value, out var pName))
                {
                    parentName = pName;
                }
                return e.ToDto(parentName);
            }).ToList();
        }
    }
}
