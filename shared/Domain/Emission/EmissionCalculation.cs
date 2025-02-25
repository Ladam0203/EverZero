namespace Domain.Emission;

public class EmissionCalculation
{
    public IEnumerable<ShallowInvoiceWithEmissionCalculation> Invoices { get; set; }
    public decimal TotalEmission { get; set; }
}