using System;
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
    public class DesignationService : IDesignationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDesignationRepository _designationRepository;
        private readonly IApplicationDbContext _context;

        public DesignationService(
            IUnitOfWork unitOfWork,
            IDesignationRepository designationRepository,
            IApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _designationRepository = designationRepository;
            _context = context;
        }

        public async Task<List<DesignationDto>> GetAllDesignationsAsync()
        {
            var designations = await _context.Designations
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DepartmentId", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM designations
                    WHERE "DeletedFlag" = 1
                    ORDER BY "Name"
                """)
                .AsNoTracking()
                .ToListAsync();

            var departmentsDict = await _context.Departments
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM departments
                    WHERE "DeletedFlag" = 1
                """)
                .AsNoTracking()
                .ToDictionaryAsync(d => d.Id, d => d.Name);

            return designations.ToDtoList(departmentsDict);
        }

        public async Task<DesignationDto?> GetDesignationByIdAsync(int id)
        {
            var designation = await _context.Designations
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DepartmentId", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM designations
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (designation is null) return null;

            string? departmentName = null;
            if (designation.DepartmentId.HasValue)
            {
                departmentName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM departments
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, designation.DepartmentId.Value).FirstOrDefaultAsync();
            }

            return designation.ToDto(departmentName);
        }

        public async Task<DesignationDto> CreateDesignationAsync(CreateDesignationRequest request)
        {
            var trimmedName = request.Name.Trim();
            var exists = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM designations
                WHERE "DeletedFlag" = 1 AND LOWER("Name") = LOWER({0})
            """, trimmedName).SingleOrDefaultAsync() > 0;

            if (exists)
            {
                throw new InvalidOperationException($"A designation with the name '{trimmedName}' already exists.");
            }

            string? departmentName = null;
            if (request.DepartmentId.HasValue)
            {
                departmentName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM departments
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, request.DepartmentId.Value).FirstOrDefaultAsync();

                if (departmentName is null)
                {
                    throw new ArgumentException($"Department with ID {request.DepartmentId.Value} does not exist.");
                }
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
            var designation = await _context.Designations
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DepartmentId", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM designations
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, id)
                .FirstOrDefaultAsync();

            if (designation is null || designation.DeletedFlag == 0) return null;

            var trimmedName = request.Name.Trim();
            var exists = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM designations
                WHERE "DeletedFlag" = 1 AND "Id" != {0} AND LOWER("Name") = LOWER({1})
            """, id, trimmedName).SingleOrDefaultAsync() > 0;

            if (exists)
            {
                throw new InvalidOperationException($"A designation with the name '{trimmedName}' already exists.");
            }

            string? departmentName = null;
            if (request.DepartmentId.HasValue)
            {
                departmentName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM departments
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, request.DepartmentId.Value).FirstOrDefaultAsync();

                if (departmentName is null)
                {
                    throw new ArgumentException($"Department with ID {request.DepartmentId.Value} does not exist.");
                }
            }

            designation.UpdateDetails(trimmedName, request.Description, request.DepartmentId);

            _designationRepository.Update(designation);
            await _unitOfWork.SaveChangesAsync();

            return designation.ToDto(departmentName);
        }

        public async Task<bool> DeleteDesignationAsync(int id)
        {
            var designation = await _context.Designations
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DepartmentId", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM designations
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, id)
                .FirstOrDefaultAsync();

            if (designation is null || designation.DeletedFlag == 0) return false;

            designation.SoftDelete();
            _designationRepository.Update(designation);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
