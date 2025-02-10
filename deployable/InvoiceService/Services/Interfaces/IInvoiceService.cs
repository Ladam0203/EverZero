using InvoiceService.Core;
using InvoiceService.Core.DTOs;

namespace InvoiceService.Services;

public interface IInvoiceService
{
    Task<IEnumerable<GetInvoiceResponse>> GetAllByUserId(Guid userId);
    Task<PostInvoiceResponse> Create(Guid userId, PostInvoiceRequest invoice);
    //Task<Invoice> GetInvoice(Guid id);
    //Task<Invoice> UpdateInvoice(Invoice invoice);
    //Task DeleteInvoice(Guid id);
}