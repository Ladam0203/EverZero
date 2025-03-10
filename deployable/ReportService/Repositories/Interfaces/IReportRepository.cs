using Domain;

namespace ReportService.Repositories.Interfaces;

public interface IReportRepository
{
    public Task<Report> Create(Report report);
    public Task<IEnumerable<Report>> GetAllByUserId(Guid userId);
}