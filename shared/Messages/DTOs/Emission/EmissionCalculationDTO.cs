using System.Collections;
using Messages.DTOs.Emission;

namespace Domain.Emission;

public class EmissionCalculationDTO
{
    public IEnumerable<InvoiceCalculationDTO> Invoices { get; set; }
    public IEnumerable<ScopeCalculationDTO> Scopes { get; set; }
    public decimal TotalEmission { get; set; } = 0;
}