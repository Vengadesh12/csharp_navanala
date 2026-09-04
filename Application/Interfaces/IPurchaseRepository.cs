using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<(List<Purchase> Items, int TotalCount)> GetPurchasesPagedAsync(string? status, string? category, string? search, int page, int pageSize);

        Task<Dictionary<int, (int Count, int FirstPurchaseId)>> GetPurchaseGroupsByApprovalRequestIdAsync();

        Task<Purchase?> GetPurchaseByIdAsync(int id);

        Task<List<Purchase>> GetAllActivePurchasesAsync();

        Task<Purchase> AddPurchaseAsync(Purchase purchase);

        Task UpdatePurchaseAsync(Purchase purchase);

        Task<bool> SoftDeletePurchaseAsync(int id);
    }
}
