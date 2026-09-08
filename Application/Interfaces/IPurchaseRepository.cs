using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<(List<PurchaseModel> Items, int TotalCount)> GetPurchasesPagedAsync(string? status, string? category, string? search, int page, int pageSize);

        Task<Dictionary<int, (int Count, int FirstPurchaseId)>> GetPurchaseGroupsByApprovalRequestIdAsync();

        Task<PurchaseModel?> GetPurchaseByIdAsync(int id);

        Task<List<PurchaseModel>> GetAllActivePurchasesAsync();

        Task<PurchaseModel> AddPurchaseAsync(PurchaseModel purchase);

        Task UpdatePurchaseAsync(PurchaseModel purchase);

        Task<bool> SoftDeletePurchaseAsync(int id);
    }
}
