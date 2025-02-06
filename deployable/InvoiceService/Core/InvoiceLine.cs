namespace InvoiceService.Core;

public class InvoiceLine
{
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
}