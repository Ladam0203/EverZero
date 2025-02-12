namespace InvoiceService.Repository.Interfaces;

public class DbInitializer
{
    private readonly AppDbContext _context;

    public DbInitializer(AppDbContext context)
    {
        _context = context;
    }

    public async Task Initialize()
    {
        _context.Database.EnsureCreated();
        await _context.SaveChangesAsync();
    }

    public Task Reinitialize()
    {
        _context.Database.EnsureDeleted();
        return Initialize();
    }
}