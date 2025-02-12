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

    public async Task<IEnumerable<GetInvoiceResponse>> GetAllByUserId(Guid userId)
    {
        var invoices = await _invoiceRepository.GetAllByUserId(userId);
        
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
                Unit = l.Unit,
                EmissionFactorId = l.EmissionFactorId,
            }).ToList()
        });
    }
    
    public async Task<PostInvoiceResponse> Create(Guid userId, PostInvoiceRequest request)
    {
        // TODO: Add AutoMapper
        var invoice = new Invoice
        {
            Subject = request.Subject,
            SupplierName = request.SupplierName,
            BuyerName = request.BuyerName,
            Date = request.Date,
            UserId = userId,
            Lines = request.Lines.Select(l => new InvoiceLine
            {
                Description = l.Description,
                Quantity = l.Quantity,
                Unit = l.Unit,
                EmissionFactorId = l.EmissionFactorId
            }).ToList()
        };
        
        var createdInvoice = await _invoiceRepository.Create(invoice);
        
        // TODO: Add AutoMapper
        return new PostInvoiceResponse()
        {
            Id = createdInvoice.Id,
            Subject = createdInvoice.Subject,
            SupplierName = createdInvoice.SupplierName,
            BuyerName = createdInvoice.BuyerName,
            Date = createdInvoice.Date,
            UserId = createdInvoice.UserId,
            Lines = createdInvoice.Lines.Select(l => new PostInvoiceLineResponse()
            {
                Id = l.Id,
                Description = l.Description,
                Quantity = l.Quantity,
                Unit = l.Unit,
                EmissionFactorId = l.EmissionFactorId
            }).ToList()
        };
    }
}