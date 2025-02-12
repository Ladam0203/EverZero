using EmissionService.Domain;
using EmissionService.Repositories.Interfaces;
using EmissionService.Services.Interfaces;

namespace EmissionService.Services;

public class EmissionFactorService : IEmissionFactorService
{
    private readonly IEmissionFactorRepository _repository;
    
    public EmissionFactorService(IEmissionFactorRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<IEnumerable<EmissionFactor>> GetAll()
    {
        return await _repository.GetAll();
    }
}