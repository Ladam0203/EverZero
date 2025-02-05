using AuthService.Core;
using AuthService.Identity;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Repository;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly RoleManager<Role> _roleManager;

    public DbInitializer(AppDbContext context, RoleManager<Role> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public async Task Initialize()
    {
        _context.Database.EnsureCreated();
        
        if (!_context.Roles.Any()) {
            await _roleManager.CreateAsync(new Role() {
                Name = "User"
            });
            await _roleManager.CreateAsync(new Role() {
                Name = "Admin"
            });
        }
        
        await _context.SaveChangesAsync();
    }
    
    public Task Reinitialize()
    {
        _context.Database.EnsureDeleted();
        return Initialize();
    }
}