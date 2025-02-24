using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace ReportService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GenerateReport(Guid userId)
    {
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
        return Ok(new { url = downloadUrl });
    }
}