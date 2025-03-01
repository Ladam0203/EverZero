using Domain.Emission;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ReportService.Services;

public class ReportService : IReportService
{
    public async Task<string> GeneratePdfReport(Guid userId, EmissionCalculationDTO dto)
    {
        // Validate invoices belong to user
        if (dto.Invoices.Any(i => i.UserId != userId))
        {
            throw new UnauthorizedAccessException(
                "Reports can only be made out of invoices belonging to the user. Heeey, how did you get them anyway? \ud83e\udd14");
            // TODO: Log this
        }

        var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
        Directory.CreateDirectory(reportPath); // Creates directory if it doesn't exist

        var fileName = $"Report-{userId}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(reportPath, fileName);

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.Header().Height(50).Background(Colors.Grey.Lighten3)
                    .AlignCenter().Text($"Emission Report")
                    .FontSize(20).Bold().FontColor(Colors.Blue.Medium);

                page.Content().Column(column =>
                {
                    // Summary Section
                    column.Item().PaddingVertical(10).Text("Summary")
                        .FontSize(16).Bold().FontColor(Colors.Black);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Description");
                            header.Cell().Element(CellStyle).Text("Value");
                        });

                        table.Cell().Element(CellStyle).Text("Total Invoices");
                        table.Cell().Element(CellStyle).Text(dto.Invoices.Count().ToString());

                        table.Cell().Element(CellStyle).Text("Total Emission");
                        table.Cell().Element(CellStyle).Text($"{dto.TotalEmission:F2} units");
                    });

                    // Invoices Details Section
                    column.Item().PaddingTop(20).Text("Invoice Details")
                        .FontSize(16).Bold().FontColor(Colors.Black);

                    foreach (var invoice in dto.Invoices)
                    {
                        column.Item().PaddingVertical(10).Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(
                            invoiceColumn =>
                            {
                                // Invoice Header
                                invoiceColumn.Item().Text($"Invoice: {invoice.Subject}")
                                    .FontSize(14).Bold();

                                invoiceColumn.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Field");
                                        header.Cell().Element(CellStyle).Text("Details");
                                        header.Cell().Element(CellStyle).Text("Emission");
                                    });

                                    table.Cell().Element(CellStyle).Text("Supplier");
                                    table.Cell().Element(CellStyle).Text(invoice.SupplierName);
                                    table.Cell().Element(CellStyle).Text("");

                                    table.Cell().Element(CellStyle).Text("Buyer");
                                    table.Cell().Element(CellStyle).Text(invoice.BuyerName);
                                    table.Cell().Element(CellStyle).Text("");

                                    table.Cell().Element(CellStyle).Text("Date");
                                    table.Cell().Element(CellStyle).Text(invoice.Date.ToString("yyyy-MM-dd"));
                                    table.Cell().Element(CellStyle).Text("");

                                    table.Cell().Element(CellStyle).Text("Total Emission");
                                    table.Cell().Element(CellStyle).Text("");
                                    table.Cell().Element(CellStyle).Text($"{invoice.Emission:F2}");
                                });

                                // Invoice Lines
                                if (invoice.Lines.Any())
                                {
                                    invoiceColumn.Item().PaddingTop(10).Text("Invoice Lines")
                                        .FontSize(12).Bold();

                                    invoiceColumn.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("Description");
                                            header.Cell().Element(CellStyle).Text("Quantity");
                                            header.Cell().Element(CellStyle).Text("Unit");
                                            header.Cell().Element(CellStyle).Text("Emission");
                                        });

                                        foreach (var line in invoice.Lines)
                                        {
                                            table.Cell().Element(CellStyle).Text(line.Description);
                                            table.Cell().Element(CellStyle).Text(line.Quantity.ToString("F2"));
                                            table.Cell().Element(CellStyle).Text(line.Unit);
                                            table.Cell().Element(CellStyle).Text($"{line.Emission:F2}");
                                        }
                                    });
                                }
                            });
                    }
                });

                page.Footer().AlignCenter()
                    .Text(txt =>
                    {
                        txt.Span("Page ");
                        txt.CurrentPageNumber();
                        txt.Span(" of ");
                        txt.TotalPages();
                    });
            });
        }).GeneratePdf();

        await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
        return $"/reports/{fileName}";

        // Helper method for table cell styling
        static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .AlignLeft()
                .AlignMiddle();
        }
    }
}