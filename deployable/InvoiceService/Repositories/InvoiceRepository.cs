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
        return await _context.Invoices
            .Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate)
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
    
    public Task<List<Invoice>> CreateAll(List<Invoice> invoices)
    {
        _context.Invoices.AddRange(invoices);
        _context.SaveChanges();
        return Task.FromResult(invoices);
    }
    
    public async Task Update(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
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