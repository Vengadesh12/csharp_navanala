using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    /// <summary>
    /// Implements Department catalog management, hierarchical aggregation, and designation mapping.
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;

        public DepartmentService(IUnitOfWork unitOfWork, IApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<DepartmentOverviewResponse> GetDepartmentsOverviewAsync()
        {
            var departments = await _unitOfWork.Departments.GetActiveDepartmentsWithDesignationsAsync();
            var allDesignations = await _unitOfWork.Designations.GetActiveDesignationsAsync();
            var allUsers = await _unitOfWork.Users.GetAllUsersAsync();

            // Calculate user count per designation
            var userCountByDesignation = allUsers
                .Where(u => u.DesignationId.HasValue && u.DeletedFlag == 1)
                .GroupBy(u => u.DesignationId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var departmentDtos = departments.Select(d =>
            {
                var mappedDes = d.Designations.Where(des => des.DeletedFlag == 1).ToList();
                var userCount = mappedDes.Sum(des => userCountByDesignation.TryGetValue(des.Id, out var cnt) ? cnt : 0);
                var desDtos = mappedDes.Select(des => des.ToDto(d.Name, userCountByDesignation.TryGetValue(des.Id, out var cnt) ? cnt : 0)).ToList();

                return d.ToDto(userCount, desDtos);
            }).ToList();

            var activeDeptIds = departments.Select(d => d.Id).ToHashSet();
            var unassignedDesignations = allDesignations
                .Where(des => !des.DepartmentId.HasValue || !activeDeptIds.Contains(des.DepartmentId.Value))
                .Select(des => des.ToDto(null, userCountByDesignation.TryGetValue(des.Id, out var cnt) ? cnt : 0))
                .ToList();

            return new DepartmentOverviewResponse
            {
                TotalDepartments = departments.Count,
                TotalDesignations = allDesignations.Count,
                MappedDesignations = allDesignations.Count - unassignedDesignations.Count,
                UnassignedDesignations = unassignedDesignations.Count,
                Departments = departmentDtos,
                UnassignedList = unassignedDesignations
            };
        }

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var overview = await GetDepartmentsOverviewAsync();
            return overview.Departments;
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetActiveDepartmentByIdAsync(id);
            if (department is null) return null;

            var allUsers = await _unitOfWork.Users.GetAllUsersAsync();
            var mappedDes = department.Designations.Where(des => des.DeletedFlag == 1).ToList();
            var desIds = mappedDes.Select(des => des.Id).ToHashSet();
            var userCount = allUsers.Count(u => u.DesignationId.HasValue && desIds.Contains(u.DesignationId.Value) && u.DeletedFlag == 1);

            return department.ToDto(userCount);
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request)
        {
            var trimmedName = request.Name.Trim();
            if (await _unitOfWork.Departments.DepartmentExistsByNameAsync(trimmedName))
            {
                throw new InvalidOperationException($"A department named '{trimmedName}' already exists.");
            }

            var department = Department.Create(trimmedName, request.Description);

            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            // Map requested designations if any
            if (request.DesignationIds != null && request.DesignationIds.Count > 0)
            {
                var designations = await _context.Designations
                    .Where(d => request.DesignationIds.Contains(d.Id) && d.DeletedFlag == 1)
                    .ToListAsync();

                foreach (var des in designations)
                {
                    des.AssignDepartment(department.Id);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            return (await GetDepartmentByIdAsync(department.Id))!;
        }

        public async Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department is null || department.DeletedFlag == 0) return null;

            var trimmedName = request.Name.Trim();
            if (await _unitOfWork.Departments.DepartmentExistsByNameAsync(trimmedName, id))
            {
                throw new InvalidOperationException($"A department named '{trimmedName}' already exists.");
            }

            department.UpdateDetails(trimmedName, request.Description);
            _unitOfWork.Departments.Update(department);

            // If DesignationIds is provided, update mappings
            if (request.DesignationIds != null)
            {
                // Clear existing designations belonging to this department that are not in the new list
                var existingDesignations = await _context.Designations
                    .Where(d => d.DepartmentId == id && d.DeletedFlag == 1)
                    .ToListAsync();

                foreach (var des in existingDesignations)
                {
                    if (!request.DesignationIds.Contains(des.Id))
                    {
                        des.UnassignDepartment();
                    }
                }

                // Assign new designations to this department
                var newDesignations = await _context.Designations
                    .Where(d => request.DesignationIds.Contains(d.Id) && d.DeletedFlag == 1)
                    .ToListAsync();

                foreach (var des in newDesignations)
                {
                    des.AssignDepartment(id);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetDepartmentByIdAsync(id);
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department is null || department.DeletedFlag == 0) return false;

            department.SoftDelete();
            _unitOfWork.Departments.Update(department);

            // Unassign all designations under this department
            var assignedDesignations = await _context.Designations
                .Where(d => d.DepartmentId == id)
                .ToListAsync();

            foreach (var des in assignedDesignations)
            {
                des.UnassignDepartment();
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<DepartmentDto?> MapDesignationsToDepartmentAsync(int departmentId, MapDepartmentDesignationsRequest request)
        {
            var department = await _unitOfWork.Departments.GetActiveDepartmentByIdAsync(departmentId);
            if (department is null) return null;

            if (request.DesignationIds != null && request.DesignationIds.Count > 0)
            {
                var designations = await _context.Designations
                    .Where(d => request.DesignationIds.Contains(d.Id) && d.DeletedFlag == 1)
                    .ToListAsync();

                foreach (var des in designations)
                {
                    des.AssignDepartment(departmentId);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            return await GetDepartmentByIdAsync(departmentId);
        }
    }
}
