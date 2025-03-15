using AutoMapper;

using EmissionService.Domain;
using EmissionService.Repositories.Interfaces;
using EmissionService.Services.Interfaces;
using ILogger = Serilog.ILogger;

namespace EmissionService.Services;

public class EmissionService : IEmissionFactorService
{
    private readonly IEmissionFactorRepository _repository;
    private readonly IMapper _mapper;
    
    private readonly ILogger _logger;
    
    public EmissionService(IEmissionFactorRepository repository, IMapper mapper, ILogger logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<IEnumerable<EmissionFactor>> GetAll()
    {
        return await _repository.GetAll();
    }
}