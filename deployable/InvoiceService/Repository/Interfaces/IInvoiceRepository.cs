using InvoiceService.Core;

namespace InvoiceService.Repository;

public interface IInvoiceRepository
{
    public Task<IEnumerable<Invoice>> GetInvoicesByUserId(Guid userId);
    //public Task<Invoice> GetInvoice(Guid id);
    //public Task<Invoice> CreateInvoice(Invoice invoice);
}