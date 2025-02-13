using EmissionService.Domain;
using EmissionService.Repositories.Interfaces;
using MongoDB.Driver;

namespace EmissionService.Repositories;

public class EmissionFactorRepository : IEmissionFactorRepository
{
    private readonly IMongoCollection<EmissionFactor> _emissionFactors;

    public EmissionFactorRepository(IMongoDatabase database)
    {
        _emissionFactors = database.GetCollection<EmissionFactor>("emissionfactors");
    }
    
    public async Task<IEnumerable<EmissionFactor>> GetAll()
    {
        return await _emissionFactors.Find(_ => true).ToListAsync();
    }
}