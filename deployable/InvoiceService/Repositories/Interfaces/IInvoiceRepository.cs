using InvoiceService.Core;

namespace InvoiceService.Repositories.Interfaces;

public interface IInvoiceRepository
{
    public Task<IEnumerable<Invoice>> GetAllByUserId(Guid userId, DateTime startDate, DateTime endDate);
    public Task<Invoice> GetById(Guid id);
    public Task<Invoice> Create(Invoice invoice);
    public Task<List<Invoice>> CreateAll(List<Invoice> invoices);
    public Task Update(Invoice invoice);
    public Task Delete(Invoice invoice);
    
    public Task<Guid?> GetEmissionFactorIdBy(string supplierName, string invoiceLineDescription, string unit);
}