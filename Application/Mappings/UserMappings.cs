using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class UserMappings
    {
        public static UserDto ToDto(this User user, string? roleName = null, string? designationName = null)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ProfileImage = user.ProfileImage,
                Phone = user.Phone ?? string.Empty,
                Age = user.Age,
                Address = user.Address ?? string.Empty,
                RoleId = user.RoleId,
                RoleName = roleName,
                DesignationId = user.DesignationId,
                DesignationName = designationName,
                DeletedFlag = user.DeletedFlag,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public static List<UserDto> ToDtoList(
            this IEnumerable<User> users,
            IReadOnlyDictionary<int, string>? rolesDict = null,
            IReadOnlyDictionary<int, string>? designationsDict = null)
        {
            return users.Select(u =>
            {
                string? roleName = null;
                if (u.RoleId.HasValue && rolesDict != null && rolesDict.TryGetValue(u.RoleId.Value, out var rName))
                {
                    roleName = rName;
                }

                string? desName = null;
                if (u.DesignationId.HasValue && designationsDict != null && designationsDict.TryGetValue(u.DesignationId.Value, out var dName))
                {
                    desName = dName;
                }

                return u.ToDto(roleName, desName);
            }).ToList();
        }
    }
}
