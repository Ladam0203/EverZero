using System.ComponentModel.DataAnnotations;

namespace InvoiceService.Core;

public class InvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    
    public Guid? EmissionFactorId { get; set; }  // Foreign Key

    public Guid InvoiceId { get; set; }  // Foreign Key
    public Invoice Invoice { get; set; }  // Navigation Property
    
    [Timestamp]
    public byte[]? Version { get; set; }
}