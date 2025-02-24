using Domain;

namespace EmissionService.Domain.DTOs;

public class EmissionCalculationRequest
{
    public IEnumerable<ShallowInvoice> Invoices { get; set; }
}