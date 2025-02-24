using EmissionService.Domain;
using EmissionService.Domain.DTOs;
using EmissionService.Repositories.Interfaces;
using EmissionService.Services.Interfaces;
using MongoDB.Driver.Linq;

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
    
    public async Task<EmissionCalculationResponse> CalculateEmission(EmissionCalculationRequest request)
    {
        var emissionFactorIds = request.Invoices
            .SelectMany(i => i.Lines
                .Select(l => l.EmissionFactorId)
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id.Value)) // We are ignoring the null and empty values TODO: Raise an error
            .ToList();

        
        var emissionFactors = await _repository.GetByIds(emissionFactorIds);
        
        // TODO: Calculate the total emission
        
        return new EmissionCalculationResponse()
        {
            TotalEmission = 1000
        };
    }
}