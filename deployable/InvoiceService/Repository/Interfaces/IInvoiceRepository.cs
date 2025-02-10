using InvoiceService.Core;

namespace InvoiceService.Repository;

public interface IInvoiceRepository
{
    public Task<IEnumerable<Invoice>> GetAllByUserId(Guid userId);
    public Task<Invoice> Create(Invoice invoice);
    //public Task<Invoice> GetInvoice(Guid id);
}