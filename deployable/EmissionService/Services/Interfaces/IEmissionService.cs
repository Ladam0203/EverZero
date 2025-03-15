using Domain;
using Domain.Emission;
using EmissionService.Domain;

namespace EmissionService.Services.Interfaces;

public interface IEmissionFactorService
{
    Task<IEnumerable<EmissionFactor>> GetAll();
}