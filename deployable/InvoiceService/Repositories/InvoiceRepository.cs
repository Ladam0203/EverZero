using InvoiceService.Core;
using InvoiceService.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Repository;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;
    
    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Invoice>> GetAllByUserId(Guid userId)
    {
        return await _context.Invoices
            .Where(i => i.UserId == userId)
            .Include(i => i.Lines)
            .ToListAsync();
    }
    
    public Task<Invoice> Create(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        _context.SaveChanges();
        return Task.FromResult(invoice);
    }
    
    public async Task<Guid?> GetEmissionFactorIdBy(string supplierName, string invoiceLineDescription, string unit)
    {
        var emissionFactorId = await _context.Invoices
            .Where(i => i.SupplierName == supplierName)
            .SelectMany(i => i.Lines
                .Where(l => l.Description == invoiceLineDescription && l.Unit == unit)
                .Select(l => l.EmissionFactorId))
            .FirstOrDefaultAsync();

        return emissionFactorId;
    }
}