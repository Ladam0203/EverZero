namespace InvoiceService.Core.DTOs;

public class GetInvoiceLineResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public Guid? EmissionFactorId { get; set; }
}