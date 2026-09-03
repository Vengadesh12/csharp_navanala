using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class ApprovalMappings
    {
        public static ApprovalRequestDto ToDto(this ApprovalRequest entity)
        {
            return new ApprovalRequestDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                EmployeeName = entity.EmployeeName,
                EmployeeEmail = entity.EmployeeEmail,
                DepartmentName = entity.DepartmentName,
                ItemName = entity.ItemName,
                Category = entity.Category,
                Description = entity.Description,
                Quantity = entity.Quantity,
                Priority = entity.Priority,
                EstimatedAmount = entity.EstimatedAmount,
                Status = entity.Status,
                Comments = entity.Comments,
                ReviewedById = entity.ReviewedById,
                ReviewedByName = entity.ReviewedByName,
                ReviewedAt = entity.ReviewedAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedFlag = entity.DeletedFlag
            };
        }

        public static List<ApprovalRequestDto> ToDtoList(this IEnumerable<ApprovalRequest> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
