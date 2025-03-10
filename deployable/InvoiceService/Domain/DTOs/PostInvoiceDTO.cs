namespace InvoiceService.Core.DTOs;

public class PostInvoiceDTO
{
    public string Subject { get; set; }
    public string SupplierName { get; set; }
    public string BuyerName { get; set; }
    public DateTime Date { get; set; }
    
    public List<PostInvoiceLineDTO> Lines { get; set; } = new();
}