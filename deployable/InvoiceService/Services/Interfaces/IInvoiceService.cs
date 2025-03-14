
using Domain;
using InvoiceService.Core.DTOs;

namespace InvoiceService.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDTO>> GetAllByUserId(Guid userId, DateTime startDate, DateTime endDate);
    Task<InvoiceDTO> Create(Guid userId, PostInvoiceDTO invoice);
    Task Delete(Guid userId, Guid id);
}