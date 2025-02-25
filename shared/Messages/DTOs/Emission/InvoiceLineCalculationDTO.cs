namespace Domain;

public class InvoiceLineCalculationDTO
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    
    public Guid? EmissionFactorId { get; set; }
    
    public decimal Emission { get; set; } = 0;
}