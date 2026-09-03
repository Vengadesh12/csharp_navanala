using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DepartmentOverviewResponse> GetDepartmentsOverviewAsync()
        {
            var departments = await _unitOfWork.Departments.GetActiveDepartmentsWithDesignationsAsync();
            var allDesignations = await _unitOfWork.Designations.GetActiveDesignationsAsync();
            var allUsers = await _unitOfWork.Users.GetAllUsersAsync();

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
            var exists = await _unitOfWork.Departments.DepartmentExistsByNameAsync(trimmedName);

            if (exists)
            {
                throw new InvalidOperationException($"A department named '{trimmedName}' already exists.");
            }

            var department = Department.Create(trimmedName, request.Description);

            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            if (request.DesignationIds != null && request.DesignationIds.Count > 0)
            {
                var designations = await _unitOfWork.Designations.GetDesignationsByIdsAsync(request.DesignationIds);

                foreach (var des in designations)
                {
                    des.AssignDepartment(department.Id);
                    _unitOfWork.Designations.Update(des);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            return (await GetDepartmentByIdAsync(department.Id))!;
        }

        public async Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request)
        {
            var department = await _unitOfWork.Departments.GetActiveDepartmentByIdAsync(id);
            if (department is null || department.DeletedFlag == 0) return null;

            var trimmedName = request.Name.Trim();
            var exists = await _unitOfWork.Departments.DepartmentExistsByNameAsync(trimmedName, id);

            if (exists)
            {
                throw new InvalidOperationException($"A department named '{trimmedName}' already exists.");
            }

            department.UpdateDetails(trimmedName, request.Description);
            _unitOfWork.Departments.Update(department);

            if (request.DesignationIds != null)
            {
                var existingDesignations = await _unitOfWork.Designations.GetDesignationsByDepartmentIdAsync(id);

                foreach (var des in existingDesignations)
                {
                    if (!request.DesignationIds.Contains(des.Id))
                    {
                        des.UnassignDepartment();
                        _unitOfWork.Designations.Update(des);
                    }
                }

                var newDesignations = await _unitOfWork.Designations.GetDesignationsByIdsAsync(request.DesignationIds);

                foreach (var des in newDesignations)
                {
                    des.AssignDepartment(id);
                    _unitOfWork.Designations.Update(des);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetDepartmentByIdAsync(id);
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetActiveDepartmentByIdAsync(id);
            if (department is null || department.DeletedFlag == 0) return false;

            department.SoftDelete();
            _unitOfWork.Departments.Update(department);

            var assignedDesignations = await _unitOfWork.Designations.GetDesignationsByDepartmentIdAsync(id);

            foreach (var des in assignedDesignations)
            {
                des.UnassignDepartment();
                _unitOfWork.Designations.Update(des);
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
                var designations = await _unitOfWork.Designations.GetDesignationsByIdsAsync(request.DesignationIds);

                foreach (var des in designations)
                {
                    des.AssignDepartment(departmentId);
                    _unitOfWork.Designations.Update(des);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            return await GetDepartmentByIdAsync(departmentId);
        }
    }
}
