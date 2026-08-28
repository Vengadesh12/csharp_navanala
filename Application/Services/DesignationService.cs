using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    /// <summary>
    /// Implements Designation catalog management and querying using business object domain methods.
    /// </summary>
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

            return designations.Select(d => new DesignationDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description ?? string.Empty,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentId.HasValue && departmentsDict.TryGetValue(d.DepartmentId.Value, out var deptName) ? deptName : null,
                DeletedFlag = d.DeletedFlag
            }).ToList();
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

            return new DesignationDto
            {
                Id = designation.Id,
                Name = designation.Name,
                Description = designation.Description ?? string.Empty,
                DepartmentId = designation.DepartmentId,
                DepartmentName = departmentName,
                DeletedFlag = designation.DeletedFlag
            };
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

            return new DesignationDto
            {
                Id = designation.Id,
                Name = designation.Name,
                Description = designation.Description ?? string.Empty,
                DepartmentId = designation.DepartmentId,
                DepartmentName = departmentName,
                DeletedFlag = designation.DeletedFlag
            };
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

            return new DesignationDto
            {
                Id = designation.Id,
                Name = designation.Name,
                Description = designation.Description ?? string.Empty,
                DepartmentId = designation.DepartmentId,
                DepartmentName = departmentName,
                DeletedFlag = designation.DeletedFlag
            };
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
