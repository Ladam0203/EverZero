namespace Domain.Emission;

public class EmissionCalculationDTO
{
    public IEnumerable<InvoiceCalculationDTO> Invoices { get; set; }
    public decimal TotalEmission { get; set; } = 0;
}