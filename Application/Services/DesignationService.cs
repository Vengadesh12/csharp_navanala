using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class DesignationService : IDesignationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDesignationRepository _designationRepository;

        public DesignationService(IUnitOfWork unitOfWork, IDesignationRepository designationRepository)
        {
            _unitOfWork = unitOfWork;
            _designationRepository = designationRepository;
        }

        public async Task<List<DesignationDto>> GetAllDesignationsAsync()
        {
            var designations = await _designationRepository.GetActiveDesignationsAsync();
            var departmentsDict = await _unitOfWork.Departments.GetDepartmentNameDictionaryAsync();

            return designations.ToDtoList(departmentsDict);
        }

        public async Task<DesignationDto?> GetDesignationByIdAsync(int id)
        {
            var designation = await _designationRepository.GetActiveDesignationByIdAsync(id);
            if (designation is null) return null;

            string? departmentName = null;
            if (designation.DepartmentId.HasValue)
            {
                var dept = await _unitOfWork.Departments.GetByIdAsync(designation.DepartmentId.Value);
                departmentName = dept?.Name;
            }

            return designation.ToDto(departmentName);
        }

        public async Task<DesignationDto> CreateDesignationAsync(CreateDesignationRequest request)
        {
            var trimmedName = request.Name.Trim();
            var existing = await _designationRepository.GetActiveDesignationsAsync();
            if (existing.Any(d => d.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A designation with the name '{trimmedName}' already exists.");
            }

            string? departmentName = null;
            if (request.DepartmentId.HasValue)
            {
                var dept = await _unitOfWork.Departments.GetActiveDepartmentByIdAsync(request.DepartmentId.Value);
                if (dept is null)
                {
                    throw new ArgumentException($"Department with ID {request.DepartmentId.Value} does not exist.");
                }
                departmentName = dept.Name;
            }

            var designation = Designation.Create(
                name: trimmedName,
                description: request.Description,
                departmentId: request.DepartmentId
            );

            await _designationRepository.AddAsync(designation);
            await _unitOfWork.SaveChangesAsync();

            return designation.ToDto(departmentName);
        }

        public async Task<DesignationDto?> UpdateDesignationAsync(int id, UpdateDesignationRequest request)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation is null || designation.DeletedFlag == 0) return null;

            var trimmedName = request.Name.Trim();
            var existing = await _designationRepository.GetActiveDesignationsAsync();
            if (existing.Any(d => d.Id != id && d.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A designation with the name '{trimmedName}' already exists.");
            }

            string? departmentName = null;
            if (request.DepartmentId.HasValue)
            {
                var dept = await _unitOfWork.Departments.GetActiveDepartmentByIdAsync(request.DepartmentId.Value);
                if (dept is null)
                {
                    throw new ArgumentException($"Department with ID {request.DepartmentId.Value} does not exist.");
                }
                departmentName = dept.Name;
            }

            designation.UpdateDetails(trimmedName, request.Description, request.DepartmentId);

            _designationRepository.Update(designation);
            await _unitOfWork.SaveChangesAsync();

            return designation.ToDto(departmentName);
        }

        public async Task<bool> DeleteDesignationAsync(int id)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation is null || designation.DeletedFlag == 0) return false;

            designation.SoftDelete();
            _designationRepository.Update(designation);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
