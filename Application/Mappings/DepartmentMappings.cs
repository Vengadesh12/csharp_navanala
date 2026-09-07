using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class DepartmentMappings
    {
        public static DepartmentDto ToDto(this Department department, int userCount = 0, List<DesignationDto>? designations = null, int activeUserCount = 0, int deletedUserCount = 0)
        {
            var desList = designations ?? department.Designations
                .Where(d => d.DeletedFlag == 1)
                .Select(d => d.ToDto(department.Name))
                .ToList();

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description ?? string.Empty,
                DeletedFlag = department.DeletedFlag,
                CreatedAt = department.CreatedAt,
                DesignationCount = desList.Count,
                UserCount = userCount,
                ActiveUserCount = activeUserCount,
                DeletedUserCount = deletedUserCount,
                Designations = desList
            };
        }
    }
}
