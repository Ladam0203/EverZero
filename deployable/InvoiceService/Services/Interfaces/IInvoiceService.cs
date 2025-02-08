using InvoiceService.Core;

namespace InvoiceService.Services;

public interface IInvoiceService
{
    Task<IEnumerable<Invoice>> GetInvoicesByUserId(Guid userId);
    //Task<Invoice> GetInvoice(Guid id);
    //Task<Invoice> CreateInvoice(Invoice invoice);
    //Task<Invoice> UpdateInvoice(Invoice invoice);
    //Task DeleteInvoice(Guid id);
}