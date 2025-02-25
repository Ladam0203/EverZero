namespace InvoiceService.Core.DTOs;

public class PostInvoiceLineDTO
{
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    
    public Guid? EmissionFactorId { get; set; }
}