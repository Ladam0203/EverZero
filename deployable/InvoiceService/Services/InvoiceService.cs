using InvoiceService.Core;
using InvoiceService.Core.DTOs;
using InvoiceService.Repository;

namespace InvoiceService.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<GetInvoiceResponse>> GetInvoicesByUserId(Guid userId)
    {
        var invoices = await _invoiceRepository.GetInvoicesByUserId(userId);
        
        // TODO: Add AutoMapper
        return invoices.Select(i => new GetInvoiceResponse
        {
            Id = i.Id,
            Subject = i.Subject,
            SupplierName = i.SupplierName,
            BuyerName = i.BuyerName,
            Date = i.Date,
            UserId = i.UserId,
            Lines = i.Lines.Select(l => new GetInvoiceLineResponse
            {
                Id = l.Id,
                InvoiceId = l.InvoiceId,
                Description = l.Description,
                Quantity = l.Quantity,
                Unit = l.Unit
            }).ToList()
        });
    }
}