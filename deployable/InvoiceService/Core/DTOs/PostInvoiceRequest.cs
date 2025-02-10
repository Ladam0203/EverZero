namespace InvoiceService.Core.DTOs;

public class PostInvoiceRequest
{
    public string Subject { get; set; }
    public string SupplierName { get; set; }
    public string BuyerName { get; set; }
    public DateTime Date { get; set; }
    
    public List<InvoiceLine> Lines { get; set; } = new();
}