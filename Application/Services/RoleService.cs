using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    /// <summary>
    /// Implements role creation, updates, querying, and soft deletion using repositories and business object domain methods.
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _unitOfWork.Roles.GetActiveRolesAsync();

            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description ?? string.Empty,
                DeletedFlag = r.DeletedFlag
            }).ToList();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _unitOfWork.Roles.GetActiveRoleByIdAsync(id);
            if (role is null) return null;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description ?? string.Empty,
                DeletedFlag = role.DeletedFlag
            };
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
        {
            var role = Role.Create(request.Name, request.Description);

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description ?? string.Empty,
                DeletedFlag = role.DeletedFlag
            };
        }

        public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request)
        {
            var role = await _unitOfWork.Roles.GetActiveRoleByIdAsync(id);
            if (role is null) return null;

            role.UpdateDetails(request.Name, request.Description);

            _unitOfWork.Roles.Update(role);
            await _unitOfWork.SaveChangesAsync();

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description ?? string.Empty,
                DeletedFlag = role.DeletedFlag
            };
        }

        public async Task<bool> SoftDeleteRoleAsync(int id)
        {
            return await _unitOfWork.Roles.SetDeletedFlagAsync(id, 0);
        }

        public async Task<bool> RestoreRoleAsync(int id)
        {
            return await _unitOfWork.Roles.SetDeletedFlagAsync(id, 1);
        }
    }
}
