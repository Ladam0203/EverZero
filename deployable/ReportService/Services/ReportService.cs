using Domain.Emission;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace ReportService.Services;

public class ReportService : IReportService
{
    public async Task<string> GeneratePdfReport(Guid userId, EmissionCalculationDTO dto)
    {
        // Check if all invoices belong to the user
        if (dto.Invoices.Any(i => i.UserId != userId))
        {
            throw new UnauthorizedAccessException("Reports can only be made out of invoices belonging to the user. Heeey, how did you get them anyway? \ud83e\udd14");
            // TODO: Log this
        }
        
        var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");

        if (!Directory.Exists(reportPath))
        {
            Directory.CreateDirectory(reportPath);
        }

        var fileName = $"Report-{userId}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(reportPath, fileName);

        // Real PDF content
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.Content().AlignCenter().AlignMiddle().Text($"Report for user {userId}")
                    .FontSize(20).FontColor(Colors.Blue.Medium);
            });
        }).GeneratePdf();

        await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

        var downloadUrl = $"/reports/{fileName}";
        
        return downloadUrl;
    }
}