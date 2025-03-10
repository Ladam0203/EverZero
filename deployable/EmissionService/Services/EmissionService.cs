using AutoMapper;
using Domain;
using Domain.Emission;
using EmissionService.Domain;
using EmissionService.Repositories.Interfaces;
using EmissionService.Services.Interfaces;
using Messages.DTOs.Emission;
using MongoDB.Driver.Linq;
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

    public async Task<EmissionCalculationDTO> CalculateEmission(Guid userId, IEnumerable<InvoiceDTO> invoices)
    {
        // TODO: Add FluentValidation
        if (invoices == null)
        {
            throw new ArgumentNullException(nameof(invoices));
        }

        if (!invoices.Any())
        {
            throw new ArgumentException("Invoices cannot be empty", nameof(invoices));
        }

        // Check if any invoice line has an emission factor
        if (!invoices.SelectMany(i => i.Lines).Any(l => l.EmissionFactorId.HasValue))
        {
            throw new ArgumentException("No emission factors found in one or more invoice lines", nameof(invoices));
        }
        
        // Check if all invoices belong to the user
        if (invoices.Any(i => i.UserId != userId))
        {
            _logger.Warning("User with ID {UserId1} tried to calculate emissions for invoices that belong to another user with ID {UserId2}", userId, invoices.First().UserId);
            throw new UnauthorizedAccessException("Calculations can only be done on invoices belonging to the user. Heeey, how did you get them anyway? \ud83e\udd14");
        }

        // Fetch the emission factors present in the invoices
        var emissionFactors = await GetEmissionFactors(invoices);

        var emissionCalculation = new EmissionCalculationDTO();
        // Per invoice 
        var invoiceCalculations = CalculateEmissionsPerInvoice(invoices, emissionFactors);
        emissionCalculation.Invoices = invoiceCalculations; 
        // Per scope
        emissionCalculation.Scopes = CalculateEmissionsPerScope(invoiceCalculations, emissionFactors);
        
        // Total emission
        var totalEmission = emissionCalculation.Invoices.Sum(i => i.Emission);
        emissionCalculation.TotalEmission = totalEmission;
        
        return emissionCalculation;
    }
    
    private async Task<IEnumerable<EmissionFactor>> GetEmissionFactors(IEnumerable<InvoiceDTO> invoices)
    {
        var emissionFactorIds = invoices
            .SelectMany(i => i.Lines
                .Select(l => l.EmissionFactorId!.Value)) // Safe to use ! here
            .ToList();
        return await _repository.GetByIds(emissionFactorIds);
    }
    
    
    private List<InvoiceCalculationDTO> CalculateEmissionsPerInvoice(IEnumerable<InvoiceDTO> invoices, IEnumerable<EmissionFactor> emissionFactors)
    {
        var invoiceCalculations = new List<InvoiceCalculationDTO>();
        foreach (var invoice in invoices)
        {
            var invoiceCalculation = _mapper.Map<InvoiceCalculationDTO>(invoice);
            foreach (var line in invoice.Lines)
            {
                var emissionFactor = emissionFactors.FirstOrDefault(ef => ef.Id == line.EmissionFactorId);
                if (emissionFactor == null)
                {
                    throw new InvalidOperationException($"Emission factor with ID {line.EmissionFactorId} not found");
                }

                var lineCalculation = _mapper.Map<InvoiceLineCalculationDTO>(line);
                lineCalculation.Emission = line.Quantity * emissionFactor.CarbonEmissionKg;
                invoiceCalculation.Lines.Add(lineCalculation);
            }
            invoiceCalculation.Emission = invoiceCalculation.Lines.Sum(l => l.Emission);
            invoiceCalculations.Add(invoiceCalculation);
        }
        return invoiceCalculations;
    }
    
    private List<ScopeCalculationDTO> CalculateEmissionsPerScope(IEnumerable<InvoiceCalculationDTO> invoiceCalculations, IEnumerable<EmissionFactor> emissionFactors)
    {
        // Create a lookup for emission factors (still improves performance)
        var emissionFactorLookup = emissionFactors.ToDictionary(ef => ef.Id, ef => ef);

        // Calculate total emission
        var totalEmission = invoiceCalculations.Sum(i => i.Emission);

        // Group emissions by scope
        var scopeGroups = invoiceCalculations
            .SelectMany(i => i.Lines)
            .GroupBy(line => emissionFactorLookup[line.EmissionFactorId!.Value].EmissionFactorMetadata.Scope)
            .Select(g => new ScopeCalculationDTO
            {
                Scope = g.Key,
                Emission = g.Sum(line => line.Emission),
                Percentage = totalEmission > 0 
                    ? (g.Sum(line => line.Emission) / totalEmission) * 100 
                    : 0,
                Categories = CalculateEmissionsPerCategory(invoiceCalculations, g, emissionFactors)
            })
            .ToList();

        return scopeGroups;
    }
    
    private List<CategoryCalculationDTO> CalculateEmissionsPerCategory(IEnumerable<InvoiceCalculationDTO> invoiceCalculations, IEnumerable<InvoiceLineCalculationDTO> invoiceLineCalculations, IEnumerable<EmissionFactor> emissionFactors)
    {
        // Create a lookup for emission factors (still improves performance)
        var emissionFactorLookup = emissionFactors.ToDictionary(ef => ef.Id, ef => ef);

        // Calculate total emission
        var totalEmission = invoiceCalculations.Sum(i => i.Emission);

        // Group emissions by category
        var categoryGroups = invoiceLineCalculations
            .GroupBy(line => emissionFactorLookup[line.EmissionFactorId!.Value].Category)
            .Select(g => new CategoryCalculationDTO
            {
                Category = g.Key,
                Emission = g.Sum(line => line.Emission),
                Percentage = totalEmission > 0 
                    ? (g.Sum(line => line.Emission) / totalEmission) * 100 
                    : 0
            })
            .ToList();

        return categoryGroups;
    }
}