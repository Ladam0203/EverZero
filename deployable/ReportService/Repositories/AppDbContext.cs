using Microsoft.EntityFrameworkCore;
using Domain;

namespace ReportService.Repositories
{
    public class AppDbContext : DbContext
    {
        public DbSet<Report> Reports { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}