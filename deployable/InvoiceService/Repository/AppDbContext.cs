using InvoiceService.Core;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Repository.Interfaces;

public class AppDbContext : DbContext
{
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLine> InvoiceLines { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>()
            .Property(i => i.Id)
            .HasDefaultValue(Guid.NewGuid());

        modelBuilder.Entity<InvoiceLine>()
            .Property(l => l.Id)
            .HasDefaultValue(Guid.NewGuid());
        
        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Lines)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId);
        
        base.OnModelCreating(modelBuilder);
    }
}