using EmissionService.Domain;

namespace EmissionService.Repositories.Interfaces;

public interface IEmissionFactorRepository
{
    public Task<IEnumerable<EmissionFactor>> GetAll();
}