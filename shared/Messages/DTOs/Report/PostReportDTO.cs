using Domain.Emission;

namespace Messages.DTOs.Report;

public class PostReportDTO
{
    public EmissionCalculationDTO EmissionCalculation { get; set; }
    public bool ShouldIncludePerInvoiceEmissionDetails { get; set; }
}