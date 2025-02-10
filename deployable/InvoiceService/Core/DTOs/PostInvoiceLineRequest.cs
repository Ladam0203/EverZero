namespace InvoiceService.Core.DTOs;

public class PostInvoiceLineRequest
{
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
}