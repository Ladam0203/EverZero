namespace AuthService.Repository;

public class DbInitializer
{
    private readonly AppDbContext _context;

    public DbInitializer(AppDbContext context)
    {
        _context = context;
    }

    public void Initialize()
    {
        _context.Database.EnsureCreated();
    }
    
    public void Reinitialize()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }
}