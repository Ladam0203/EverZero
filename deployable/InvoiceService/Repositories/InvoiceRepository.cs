using InvoiceService.Core;
using InvoiceService.Repositories.Interfaces;
using InvoiceService.Repository;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;
    
    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Invoice>> GetAllByUserId(Guid userId, DateTime startDate, DateTime endDate)
    {
        var utcStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        return await _context.Invoices
            .Where(i => i.UserId == userId && i.Date >= utcStartDate && i.Date <= utcEndDate)
            .Include(i => i.Lines)
            .ToListAsync();
    }
    
    public async Task<Invoice> GetById(Guid id)
    {
        return await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id) ?? throw new KeyNotFoundException();
    }
    
    public Task<Invoice> Create(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        _context.SaveChanges();
        return Task.FromResult(invoice);
    }
    
    public async Task Delete(Invoice invoice)
    {
        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
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