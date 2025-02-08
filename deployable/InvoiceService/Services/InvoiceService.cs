using InvoiceService.Core;
using InvoiceService.Repository;

namespace InvoiceService.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByUserId(Guid userId)
    {
        return await _invoiceRepository.GetInvoicesByUserId(userId);
    }
}