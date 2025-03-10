using EmissionService.Domain;

namespace EmissionService.Repositories.Interfaces;

public interface IEmissionFactorRepository
{
    public Task<IEnumerable<EmissionFactor>> GetAll();
    public Task<EmissionFactor> GetById(Guid id);
    public Task<IEnumerable<EmissionFactor>> GetByIds(IEnumerable<Guid> ids);
}