using AutoMapper;
using Domain;
using InvoiceService.Core;
using InvoiceService.Core.DTOs;
using InvoiceService.Repository;

namespace InvoiceService.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMapper _mapper;

    public InvoiceService(IInvoiceRepository invoiceRepository, IMapper mapper)
    {
        _invoiceRepository = invoiceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InvoiceDTO>> GetAllByUserId(Guid userId)
    {
        var invoices = await _invoiceRepository.GetAllByUserId(userId);
        
        var invoiceDTOs = invoices.Select(i => _mapper.Map<InvoiceDTO>(i));
        
        return invoiceDTOs;
    }
    
    public async Task<InvoiceDTO> Create(Guid userId, PostInvoiceDTO dto)
    {
        var invoice = _mapper.Map<Invoice>(dto);
        
        var createdInvoice = await _invoiceRepository.Create(invoice);
        
        return _mapper.Map<InvoiceDTO>(createdInvoice);
    }
}