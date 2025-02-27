using Domain.Emission;

namespace ReportService.Services;

public interface IReportService
{
    public Task<string> GeneratePdfReport(Guid userId, EmissionCalculationDTO dto);
}