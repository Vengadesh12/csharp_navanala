using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class DesignationService : IDesignationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DesignationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<DesignationDto>> GetAllDesignationsAsync()
        {
            var designations = await _unitOfWork.Designations.GetActiveDesignationsAsync();
            var departmentsDict = await _unitOfWork.Departments.GetDepartmentNameDictionaryAsync();

            return designations.ToDtoList(departmentsDict);
        }

        public async Task<DesignationDto?> GetDesignationByIdAsync(int id)
        {
            var designation = await _unitOfWork.Designations.GetActiveDesignationByIdAsync(id);
            if (designation is null) return null;

            string? departmentName = null;
            if (designation.DepartmentId.HasValue)
            {
                departmentName = await _unitOfWork.Designations.GetDepartmentNameByIdAsync(designation.DepartmentId.Value);
            }

            return designation.ToDto(departmentName);
        }

        public async Task<DesignationDto> CreateDesignationAsync(CreateDesignationRequest request)
        {
            var trimmedName = request.Name.Trim();
            var exists = await _unitOfWork.Designations.DesignationExistsByNameAsync(trimmedName);

            if (exists)
            {
                throw new InvalidOperationException($"A designation with the name '{trimmedName}' already exists.");
            }

            string? departmentName = null;
            if (request.DepartmentId.HasValue)
            {
                departmentName = await _unitOfWork.Designations.GetDepartmentNameByIdAsync(request.DepartmentId.Value);
                if (departmentName is null)
                {
                    throw new ArgumentException("The specified department does not exist.");
                }
            }

            var designation = Designation.Create(trimmedName, request.Description, request.DepartmentId);

            await _unitOfWork.Designations.AddAsync(designation);
            await _unitOfWork.SaveChangesAsync();

            return designation.ToDto(departmentName);
        }

        public async Task<DesignationDto?> UpdateDesignationAsync(int id, UpdateDesignationRequest request)
        {
            var designation = await _unitOfWork.Designations.GetActiveDesignationByIdAsync(id);
            if (designation is null) return null;

            var trimmedName = request.Name.Trim();
            var exists = await _unitOfWork.Designations.DesignationExistsByNameAsync(trimmedName, id);

            if (exists)
            {
                throw new InvalidOperationException($"A designation with the name '{trimmedName}' already exists.");
            }

            string? departmentName = null;
            if (request.DepartmentId.HasValue)
            {
                departmentName = await _unitOfWork.Designations.GetDepartmentNameByIdAsync(request.DepartmentId.Value);
                if (departmentName is null)
                {
                    throw new ArgumentException("The specified department does not exist.");
                }
            }

            designation.UpdateDetails(trimmedName, request.Description, request.DepartmentId);

            _unitOfWork.Designations.Update(designation);
            await _unitOfWork.SaveChangesAsync();

            return designation.ToDto(departmentName);
        }

        public async Task<bool> DeleteDesignationAsync(int id)
        {
            return await _unitOfWork.Designations.SetDeletedFlagAsync(id, 0);
        }

        public async Task<bool> RestoreDesignationAsync(int id)
        {
            return await _unitOfWork.Designations.SetDeletedFlagAsync(id, 1);
        }
    }
}
