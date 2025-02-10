namespace InvoiceService.Core.DTOs;

public class PostInvoiceLineResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
}