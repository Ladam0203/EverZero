using Domain;
using Microsoft.EntityFrameworkCore;
using ReportService.Repositories.Interfaces;

namespace ReportService.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _dbContext;

        public ReportRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Report> Create(Report report)
        {
            await _dbContext.Set<Report>().AddAsync(report);
            await _dbContext.SaveChangesAsync();
            return report;
        }

        public async Task<IEnumerable<Report>> GetAllByUserId(Guid userId)
        {
            return await _dbContext.Set<Report>()
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }
}