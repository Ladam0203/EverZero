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
    
    public async Task<EmissionFactor> GetById(Guid id)
    {
        return await _emissionFactors.Find(e => e.Id == id).FirstOrDefaultAsync();
    }
    
    public async Task<IEnumerable<EmissionFactor>> GetByIds(IEnumerable<Guid> ids)
    {
        return await _emissionFactors.Find(e => ids.Contains(e.Id)).ToListAsync();
    }
}