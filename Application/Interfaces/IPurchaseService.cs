using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service managing vendor procurement and purchase quotations for approved products.
    /// </summary>
    public interface IPurchaseService
    {
        /// <summary>
        /// Retrieves paginated list of purchases / vendor quotations with optional search, status, and category filters.
        /// </summary>
        Task<PagedPurchaseResponse> GetPurchasesAsync(PurchaseQueryParameters query);

        /// <summary>
        /// Retrieves all approved products available for vendor quotation.
        /// </summary>
        Task<List<ApprovedProductDto>> GetApprovedProductsAsync();

        /// <summary>
        /// Retrieves executive procurement summary metrics.
        /// </summary>
        Task<PurchaseSummaryDto> GetSummaryAsync();

        /// <summary>
        /// Retrieves single purchase details by ID.
        /// </summary>
        Task<PurchaseDto?> GetPurchaseByIdAsync(int id);

        /// <summary>
        /// Creates a new vendor quotation for an approved product.
        /// </summary>
        Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseRequest request, int createdByUserId, string createdByName);

        /// <summary>
        /// Updates an existing vendor quotation or procurement status.
        /// </summary>
        Task<PurchaseDto?> UpdatePurchaseAsync(int id, UpdatePurchaseRequest request);

        /// <summary>
        /// Soft-deletes a purchase quotation.
        /// </summary>
        Task<bool> DeletePurchaseAsync(int id);
    }
}
