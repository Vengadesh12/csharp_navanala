using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class DesignationMappings
    {
        public static DesignationDto ToDto(this Designation entity, string? departmentName = null, int userCount = 0)
        {
            return new DesignationDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? string.Empty,
                DepartmentId = entity.DepartmentId,
                DepartmentName = departmentName,
                DeletedFlag = entity.DeletedFlag
            };
        }

        public static List<DesignationDto> ToDtoList(this IEnumerable<Designation> entities, Dictionary<int, string>? departmentsDict = null)
        {
            return entities.Select(e =>
            {
                string? deptName = null;
                if (e.DepartmentId.HasValue && departmentsDict != null)
                {
                    departmentsDict.TryGetValue(e.DepartmentId.Value, out deptName);
                }
                return e.ToDto(deptName);
            }).ToList();
        }
    }
}
