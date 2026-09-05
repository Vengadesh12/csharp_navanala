using System;

namespace MyBackend.Domain.Entities.Model
{
    public class UserLoginDetails
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public int? RoleId { get; set; }

        public string? RoleName { get; set; }

        public int? DesignationId { get; set; }

        public string? DesignationName { get; set; }

        public string? DepartmentName { get; set; }

        public string Phone { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Address { get; set; } = string.Empty;

        public string? ProfileImage { get; set; }

        public int DeletedFlag { get; set; } = 1;

        public bool IsFirstLogin { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? PermissionsCsv { get; set; }

        public string? MenuNamesCsv { get; set; }

        public List<string> GetPermissions() =>
            string.IsNullOrWhiteSpace(PermissionsCsv)
                ? []
                : PermissionsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();

        public List<string> GetMenuNames() =>
            string.IsNullOrWhiteSpace(MenuNamesCsv)
                ? []
                : MenuNamesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();

        public User ToUser()
        {
            return new User
            {
                Id = Id,
                Name = Name,
                Email = Email,
                PasswordHash = PasswordHash,
                RoleId = RoleId,
                DesignationId = DesignationId,
                Phone = Phone,
                Age = Age,
                Address = Address,
                ProfileImage = ProfileImage,
                DeletedFlag = DeletedFlag,
                IsFirstLogin = IsFirstLogin,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
    }
}
