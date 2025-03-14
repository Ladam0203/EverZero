using Domain;
using Domain.Emission;
using Messages.DTOs.Report;

namespace ReportService.Services;

public interface IReportService
{
    public Task<Report> Create(Guid userId, PostReportDTO dto);
    public Task<IEnumerable<Report>> GetAllByUserId(Guid userId);
}