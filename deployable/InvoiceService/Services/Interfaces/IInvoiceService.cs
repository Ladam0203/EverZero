
using Domain;
using InvoiceService.Core.DTOs;

namespace InvoiceService.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDTO>> GetAllByUserId(Guid userId, DateTime startDate, DateTime endDate);
    Task<InvoiceDTO> Create(Guid userId, PostInvoiceDTO dto);
    Task<List<InvoiceDTO>> CreateAll(Guid userId, List<PostInvoiceDTO> dtos);
    Task<InvoiceDTO> Update(Guid userId, PutInvoiceDTO dto);
    Task Delete(Guid userId, Guid id);
}