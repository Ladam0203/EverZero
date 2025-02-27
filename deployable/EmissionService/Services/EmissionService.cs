using AutoMapper;
using Domain;
using Domain.Emission;
using EmissionService.Domain;
using EmissionService.Repositories.Interfaces;
using EmissionService.Services.Interfaces;
using MongoDB.Driver.Linq;

namespace EmissionService.Services;

public class EmissionService : IEmissionFactorService
{
    private readonly IEmissionFactorRepository _repository;
    private readonly IMapper _mapper;
    
    public EmissionService(IEmissionFactorRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
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
            throw new UnauthorizedAccessException("Calculations can only be done on invoices belonging to the user. Heeey, how did you get them anyway? \ud83e\udd14");
            // TODO: Log this
        }

        // Fetch the emission factors present in the invoices
        var emissionFactorIds = invoices
            .SelectMany(i => i.Lines
                .Select(l => l.EmissionFactorId!.Value)) // Safe to use ! here
            .ToList();
        var emissionFactors = await _repository.GetByIds(emissionFactorIds);

        var emissionCalculation = new EmissionCalculationDTO();
        List<InvoiceCalculationDTO> invoiceCalculations = new();
        foreach (var invoice in invoices)
        {
            var invoiceCalculation = _mapper.Map<InvoiceCalculationDTO>(invoice);
            foreach (var lineCalculation in invoiceCalculation.Lines)
            {
                var emissionFactor = emissionFactors
                    .FirstOrDefault(ef => ef.Id == lineCalculation.EmissionFactorId);
                if (emissionFactor == null)
                {
                    throw new InvalidOperationException($"Emission factor with ID {lineCalculation.EmissionFactorId} not found");
                }

                lineCalculation.Emission = lineCalculation.Quantity * emissionFactor.CarbonEmissionKg;
            }
            invoiceCalculation.Emission = invoiceCalculation.Lines.Sum(l => l.Emission);
            invoiceCalculations.Add(invoiceCalculation);
        }
        emissionCalculation.Invoices = invoiceCalculations;
        emissionCalculation.TotalEmission = emissionCalculation.Invoices.Sum(i => i.Emission);;
        
        return emissionCalculation;
    }
}