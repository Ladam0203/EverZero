using Domain;
using Domain.Emission;

namespace ReportService.Services;

public interface IReportService
{
    public Task<Report> Create(Guid userId, EmissionCalculationDTO dto);
    public Task<IEnumerable<Report>> GetAllByUserId(Guid userId);
}