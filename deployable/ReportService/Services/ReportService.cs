using Domain;
using Domain.Emission;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReportService.Repositories.Interfaces;

namespace ReportService.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;

    public ReportService(IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<Report> Create(Guid userId, EmissionCalculationDTO dto)
    {
        // Validate invoices belong to user
        if (dto.Invoices.Any(i => i.UserId != userId))
        {
            throw new UnauthorizedAccessException(
                "Reports can only be made out of invoices belonging to the user. Heeey, how did you get them anyway? \ud83e\udd14");
            // TODO: Log this
        }

        var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
        Directory.CreateDirectory(reportPath);

        var fileName = $"Report-{userId}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(reportPath, fileName);

        var invoiceDates = dto.Invoices.Select(i => i.Date);
        var startDate = invoiceDates.Min();
        var endDate = invoiceDates.Max();
        var periodString = invoiceDates.Any()
            ? $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
            : "No invoices available";

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.Header().Height(30).Background(Colors.Grey.Lighten3).Column(header =>
                {
                    header.Item().AlignCenter().Text($"Emission Report")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                });

                page.Content().Column(column =>
                {
                    // Summary Section
                    column.Item().PaddingVertical(10).Text("Summary")
                        .FontSize(16).Bold().FontColor(Colors.Black);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
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

                        table.Cell().Element(CellStyle).Text("Period");
                        table.Cell().Element(CellStyle).Text(periodString);
                    });

                    // Emissions by Scope and Category Section
                    column.Item().PaddingTop(20).Text("Emissions by Scope and Category")
                        .FontSize(16).Bold().FontColor(Colors.Black);

                    if (dto.Scopes != null && dto.Scopes.Any())
                    {
                        foreach (var scope in dto.Scopes)
                        {
                            column.Item().PaddingVertical(10).Column(scopeColumn =>
                            {
                                // Scope Header
                                scopeColumn.Item().Background(Colors.Grey.Lighten4)
                                    .Padding(5)
                                    .Text(
                                        $"{scope.Scope} (Emission: {scope.Emission:F2} units, {scope.Percentage:F2}%)")
                                    .FontSize(14).Bold();

                                // Categories Table
                                if (scope.Categories != null && scope.Categories.Any())
                                {
                                    scopeColumn.Item().PaddingLeft(20).PaddingTop(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("Category");
                                            header.Cell().Element(CellStyle).Text("Emission");
                                            header.Cell().Element(CellStyle).Text("Percentage");
                                        });

                                        foreach (var category in scope.Categories)
                                        {
                                            table.Cell().Element(CellStyle).Text(category.Category);
                                            table.Cell().Element(CellStyle).Text($"{category.Emission:F2} units");
                                            table.Cell().Element(CellStyle).Text($"{category.Percentage:F2}%");
                                        }
                                    });
                                }
                            });
                        }
                    }
                    else
                    {
                        column.Item().PaddingTop(5).Text("No scope data available")
                            .FontSize(12).Italic().FontColor(Colors.Grey.Darken1);
                    }

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

        static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .AlignLeft()
                .AlignMiddle();
        }

        string path = Path.Combine("reports", fileName);

        var report = new Report
        {
            UserId = userId,
            Path = path,
            TotalInvoices = dto.Invoices.Count(),
            TotalEmission = dto.TotalEmission,
            StartDate = startDate,
            EndDate = endDate
        };

        return await _repository.Create(report);
    }

    public async Task<IEnumerable<Report>> GetAllByUserId(Guid userId)
    {
        return await _repository.GetAllByUserId(userId);
    }
}