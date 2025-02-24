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
        
        var totalEmission = request.Invoices
            .SelectMany(i => i.Lines)
            .Where(l => l.EmissionFactorId.HasValue && l.EmissionFactorId.Value != Guid.Empty) // This also has to be provided
            .Select(l => l.Quantity * emissionFactors
                .First(ef => ef.Id == l.EmissionFactorId).EmissionFactorUnit
                .First(efu => efu.Id == l.EmissionFactorUnitId).CarbonEmissionKg)
            .Sum();
        
        return new EmissionCalculationResponse()
        {
            TotalEmission = totalEmission
        };
    }
}