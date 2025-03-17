namespace InvoiceService.Core.DTOs;

public class PutInvoiceLineDTO
{
    public Guid? Id { get; set; } = Guid.NewGuid(); // When (new) line comes from client, it will have a new ID
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    
    public Guid? EmissionFactorId { get; set; }
}