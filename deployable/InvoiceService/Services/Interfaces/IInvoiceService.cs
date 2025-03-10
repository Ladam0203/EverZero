
using Domain;
using InvoiceService.Core.DTOs;

namespace InvoiceService.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDTO>> GetAllByUserId(Guid userId);
    Task<InvoiceDTO> Create(Guid userId, PostInvoiceDTO invoice);
    //Task<Invoice> GetInvoice(Guid id);
    //Task<Invoice> UpdateInvoice(Invoice invoice);
    //Task DeleteInvoice(Guid id);
}