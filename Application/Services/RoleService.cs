using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;

        public RoleService(IUnitOfWork unitOfWork, IApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _context.Roles
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "DeletedFlag" = 1
                    ORDER BY "Id"
                """)
                .AsNoTracking()
                .ToListAsync();

            return roles.Select(r => r.ToDto()).ToList();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _context.Roles
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (role is null) return null;

            return role.ToDto();
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
        {
            var role = Role.Create(request.Name, request.Description);

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return role.ToDto();
        }

        public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request)
        {
            var role = await _context.Roles
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, id)
                .FirstOrDefaultAsync();

            if (role is null) return null;

            role.UpdateDetails(request.Name, request.Description);

            _unitOfWork.Roles.Update(role);
            await _unitOfWork.SaveChangesAsync();

            return role.ToDto();
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
