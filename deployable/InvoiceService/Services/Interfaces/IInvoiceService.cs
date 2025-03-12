
using Domain;
using InvoiceService.Core.DTOs;

namespace InvoiceService.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDTO>> GetAllByUserId(Guid userId);
    Task<InvoiceDTO> Create(Guid userId, PostInvoiceDTO invoice);
    Task Delete(Guid userId, Guid id);
    //Task<Invoice> GetInvoice(Guid id);
    //Task<Invoice> UpdateInvoice(Invoice invoice);
}