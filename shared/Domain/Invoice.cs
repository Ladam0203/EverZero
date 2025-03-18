using System.ComponentModel.DataAnnotations;
using InvoiceService.Core;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Subject { get; set; }
    public string SupplierName { get; set; }
    public string BuyerName { get; set; }
    public DateTime Date { get; set; }

    public Guid UserId { get; set; }  // Associated User

    public List<InvoiceLine> Lines { get; set; } = new();
    
    [Timestamp]
    public byte[]? Version { get; set; }
}