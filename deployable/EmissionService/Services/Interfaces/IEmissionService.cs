using EmissionService.Domain;
using EmissionService.Domain.DTOs;

namespace EmissionService.Services.Interfaces;

public interface IEmissionFactorService
{
    Task<IEnumerable<EmissionFactor>> GetAll();
    Task<EmissionCalculationResponse> CalculateEmission(EmissionCalculationRequest request);
}