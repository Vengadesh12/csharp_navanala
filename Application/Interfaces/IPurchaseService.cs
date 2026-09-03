using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IPurchaseService
    {
        Task<PagedPurchaseResponse> GetPurchasesAsync(PurchaseQueryParameters query);

        Task<List<ApprovedProductDto>> GetApprovedProductsAsync();

        Task<PurchaseSummaryDto> GetSummaryAsync();

        Task<PurchaseDto?> GetPurchaseByIdAsync(int id);

        Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseRequest request, int createdByUserId, string createdByName);

        Task<PurchaseDto?> UpdatePurchaseAsync(int id, UpdatePurchaseRequest request);

        Task<bool> DeletePurchaseAsync(int id);
    }
}
