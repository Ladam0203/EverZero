namespace Domain;

public class ShallowInvoiceLineWithEmissionCalculation
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    
    public Guid? EmissionFactorId { get; set; }
    public Guid? EmissionFactorUnitId { get; set; }
    
    public decimal TotalEmission { get; set; }
}