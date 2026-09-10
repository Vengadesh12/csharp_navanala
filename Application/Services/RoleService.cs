using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Services
{
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
            return roles.ToDtoList();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _unitOfWork.Roles.GetActiveRoleByIdAsync(id);
            if (role is null) return null;

            string? parentRoleName = null;
            if (role.ParentRoleId.HasValue)
            {
                var parentRole = await _unitOfWork.Roles.GetByIdAsync(role.ParentRoleId.Value);
                parentRoleName = parentRole?.Name;
            }

            return role.ToDto(parentRoleName);
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
        {
            var now = DateTime.UtcNow;
            var role = new RoleModel
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                ParentRoleId = request.ParentRoleId,
                DeletedFlag = 1,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return await GetRoleByIdAsync(role.Id) ?? role.ToDto();
        }

        public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request)
        {
            var role = await _unitOfWork.Roles.GetActiveRoleByIdAsync(id);
            if (role is null) return null;

            role.Name = request.Name.Trim();
            role.Description = request.Description?.Trim() ?? string.Empty;
            role.ParentRoleId = request.ParentRoleId;
            role.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Roles.Update(role);
            await _unitOfWork.SaveChangesAsync();

            return await GetRoleByIdAsync(id) ?? role.ToDto();
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
