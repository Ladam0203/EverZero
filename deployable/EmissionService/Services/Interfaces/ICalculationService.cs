using Domain;
using Domain.Emission;

namespace EmissionService.Services.Interfaces;

public interface ICalculationService
{
    public Task<EmissionCalculationDTO> CalculateEmission(Guid userId, IEnumerable<InvoiceDTO> invoices);
}