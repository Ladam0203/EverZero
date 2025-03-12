using AutoMapper;
using Domain;
using InvoiceService.Core;
using InvoiceService.Core.DTOs;
using InvoiceService.Repository;
using ILogger = Serilog.ILogger;

namespace InvoiceService.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMapper _mapper;
    
    private readonly ILogger _logger;

    public InvoiceService(IInvoiceRepository invoiceRepository, IMapper mapper, ILogger logger)
    {
        _invoiceRepository = invoiceRepository;
        _mapper = mapper;
        _logger = logger;
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
        invoice.UserId = userId;
        
        var createdInvoice = await _invoiceRepository.Create(invoice);
        
        return _mapper.Map<InvoiceDTO>(createdInvoice);
    }
    
    public async Task Delete(Guid userId, Guid id)
    {
        var invoice = await _invoiceRepository.GetById(id);
        
        if (invoice.UserId != userId) {
            _logger.Warning("User with ID {UserId1} attempted to delete invoice with ID {invoiceId} which belongs to user with ID {UserId2}", userId, id, invoice.UserId);
            throw new UnauthorizedAccessException("User not authorized to delete this invoice as it does not belong to them");
        }
        
        await _invoiceRepository.Delete(invoice);
    }
}