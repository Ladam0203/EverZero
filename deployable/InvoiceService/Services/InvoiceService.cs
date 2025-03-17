using AutoMapper;
using Domain;
using InvoiceService.Core;
using InvoiceService.Core.DTOs;
using InvoiceService.Repositories.Interfaces;
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

    public async Task<IEnumerable<InvoiceDTO>> GetAllByUserId(Guid userId, DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        var invoices = await _invoiceRepository.GetAllByUserId(userId, startDate, endDate);

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

    public async Task<InvoiceDTO> Update(Guid userId, PutInvoiceDTO dto)
    {
        var invoice = await _invoiceRepository.GetById(dto.Id);
        if (invoice == null)
        {
            throw new KeyNotFoundException($"Invoice with ID {dto.Id} not found");
        }

        // Check authorization
        if (invoice.UserId != userId)
        {
            _logger.Warning(
                "User with ID {UserId1} attempted to update invoice with ID {invoiceId} which belongs to user with ID {UserId2}",
                userId, dto.Id, invoice.UserId);
            throw new UnauthorizedAccessException(
                "User not authorized to update this invoice as it does not belong to them");
        }

        // Update the invoice
        _mapper.Map(dto, invoice);
        // Make sure InvoiceLines have the correct InvoiceId
        foreach (var line in invoice.Lines)
        {
            line.InvoiceId = invoice.Id;
        }
        
        // TODO: This throws an error when a new invoice line is added

        // Persist the updated invoice
        await _invoiceRepository.Update(invoice);

        return _mapper.Map<InvoiceDTO>(invoice);
    }

    public async Task Delete(Guid userId, Guid id)
    {
        var invoice = await _invoiceRepository.GetById(id);

        if (invoice.UserId != userId)
        {
            _logger.Warning(
                "User with ID {UserId1} attempted to delete invoice with ID {invoiceId} which belongs to user with ID {UserId2}",
                userId, id, invoice.UserId);
            throw new UnauthorizedAccessException(
                "User not authorized to delete this invoice as it does not belong to them");
        }

        await _invoiceRepository.Delete(invoice);
    }
}