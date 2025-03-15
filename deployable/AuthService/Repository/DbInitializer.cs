using AuthService.Core;
using AuthService.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace AuthService.Repository;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager; // Added UserManager

    public DbInitializer(
        AppDbContext context, 
        RoleManager<Role> roleManager,
        UserManager<User> userManager) // Added UserManager to constructor
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task Initialize()
    {
        _context.Database.EnsureCreated();
        
        // Seed Roles
        if (!_context.Roles.Any())
        {
            await _roleManager.CreateAsync(new Role()
            {
                Name = "User",
            });
            await _roleManager.CreateAsync(new Role()
            {
                Name = "Admin",
            });
        }

        // Seed User
        if (!_context.Users.Any())
        {
            var user = new User
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                UserName = "greentech.ceo",
                Email = "ceo@greentechsolutions.com",
                EmailConfirmed = true,
            };

            // Create user with a password
            var result = await _userManager.CreateAsync(user, "GreenTech#2025!");
            if (result.Succeeded)
            {
                // Assign Admin role to the user
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                throw new Exception("Failed to create initial user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        
        await _context.SaveChangesAsync();
    }
    
    public Task Reinitialize()
    {
        _context.Database.EnsureDeleted();
        return Initialize();
    }
}