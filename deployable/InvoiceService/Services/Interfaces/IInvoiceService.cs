using InvoiceService.Core;
using InvoiceService.Core.DTOs;

namespace InvoiceService.Services;

public interface IInvoiceService
{
    Task<IEnumerable<GetInvoiceResponse>> GetInvoicesByUserId(Guid userId);
    //Task<Invoice> GetInvoice(Guid id);
    //Task<Invoice> CreateInvoice(Invoice invoice);
    //Task<Invoice> UpdateInvoice(Invoice invoice);
    //Task DeleteInvoice(Guid id);
}