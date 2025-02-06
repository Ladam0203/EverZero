namespace InvoiceService.Core;

public class Invoice
{
    // public string Number { get; set; }
    public string Subject { get; set; }
    public string SupplierName { get; set; }
    // public string SupplierAddress { get; set; }
    public string BuyerName { get; set; }
    // public string BuyerAddress { get; set; }
    public DateTime Date { get; set; }
    
    public List<InvoiceLine> Lines { get; set; }
}