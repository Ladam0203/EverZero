using InvoiceService.Core;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Repository.Interfaces;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;
    
    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Invoice>> GetInvoicesByUserId(Guid userId)
    {
        return await _context.Invoices.Where(i => i.UserId == userId).ToListAsync();
    }
}