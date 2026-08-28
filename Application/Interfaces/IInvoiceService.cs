using System.Threading.Tasks;
using MyBackend.Application.Contracts;

namespace MyBackend.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<PagedInvoiceResponse> GetInvoicesAsync(InvoiceQueryParameters query);
        Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
        Task<InvoiceSummaryDto> GetSummaryAsync();
        Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, int userId, string userName, bool canEditGst);
        Task<InvoiceDto?> UpdateInvoiceAsync(int id, UpdateInvoiceRequest request, int userId, string userName, bool canEditGst);
        Task<bool> DeleteInvoiceAsync(int id, int userId);
    }
}
