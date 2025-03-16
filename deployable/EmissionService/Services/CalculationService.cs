using AutoMapper;
using Domain;
using Domain.Emission;
using EmissionService.Domain;
using EmissionService.Repositories.Interfaces;
using EmissionService.Services.Interfaces;
using Messages.DTOs.Emission;
using ILogger = Serilog.ILogger;

namespace EmissionService.Services;

public class CalculationService : ICalculationService
{
    private readonly IEmissionFactorRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public CalculationService(
        IEmissionFactorRepository repository, 
        IMapper mapper, 
        ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EmissionCalculationDTO> CalculateEmission(Guid userId, IEnumerable<InvoiceDTO> invoices)
    {
        ValidateInput(userId, invoices);
        
        var emissionFactors = await GetEmissionFactors(invoices);
        var emissionFactorLookup = emissionFactors.ToDictionary(ef => ef.Id);

        var invoiceCalculations = CalculateEmissionsPerInvoice(invoices, emissionFactorLookup);
        
        return new EmissionCalculationDTO
        {
            Invoices = invoiceCalculations,
            Scopes = CalculateEmissionsPerScope(invoiceCalculations, emissionFactorLookup),
            Years = CalculateEmissionsPerYear(invoiceCalculations, emissionFactorLookup),
            TotalEmission = invoiceCalculations.Sum(i => i.Emission)
        };
    }

    private void ValidateInput(Guid userId, IEnumerable<InvoiceDTO> invoices)
    {
        if (invoices == null)
            throw new ArgumentNullException(nameof(invoices));

        if (!invoices.Any())
            throw new ArgumentException("Invoices cannot be empty", nameof(invoices));

        if (!invoices.SelectMany(i => i.Lines).Any(l => l.EmissionFactorId.HasValue))
            throw new ArgumentException("No emission factors found in invoice lines", nameof(invoices));

        if (invoices.Any(i => i.UserId != userId))
        {
            _logger.Warning("User {UserId} attempted to calculate emissions for invoices of user {OtherUserId}", 
                userId, invoices.First().UserId);
            throw new UnauthorizedAccessException(
                "Calculations can only be performed on user's own invoices. How did you get these? 🤔");
        }
    }

    private async Task<IEnumerable<EmissionFactor>> GetEmissionFactors(IEnumerable<InvoiceDTO> invoices)
    {
        var emissionFactorIds = invoices
            .SelectMany(i => i.Lines)
            .Where(l => l.EmissionFactorId.HasValue)
            .Select(l => l.EmissionFactorId!.Value)
            .Distinct()
            .ToList();

        return await _repository.GetByIds(emissionFactorIds);
    }

    private List<InvoiceCalculationDTO> CalculateEmissionsPerInvoice(
        IEnumerable<InvoiceDTO> invoices, 
        IDictionary<Guid, EmissionFactor> emissionFactorLookup)
    {
        return invoices.Select(invoice =>
        {
            var invoiceCalc = _mapper.Map<InvoiceCalculationDTO>(invoice);
            invoiceCalc.Lines = invoice.Lines.Select(line =>
            {
                var factor = emissionFactorLookup[line.EmissionFactorId!.Value];
                var lineCalc = _mapper.Map<InvoiceLineCalculationDTO>(line);
                lineCalc.Emission = line.Quantity * factor.CarbonEmissionKg;
                return lineCalc;
            }).ToList();
            
            invoiceCalc.Emission = invoiceCalc.Lines.Sum(l => l.Emission);
            return invoiceCalc;
        }).ToList();
    }

    private List<ScopeCalculationDTO> CalculateEmissionsPerScope(
        IEnumerable<InvoiceCalculationDTO> invoiceCalcs, 
        IDictionary<Guid, EmissionFactor> emissionFactorLookup)
    {
        var totalEmission = invoiceCalcs.Sum(i => i.Emission);

        return invoiceCalcs
            .SelectMany(i => i.Lines)
            .GroupBy(l => emissionFactorLookup[l.EmissionFactorId!.Value].EmissionFactorMetadata.Scope)
            .Select(g => CreateScopeCalculation(g, totalEmission, emissionFactorLookup))
            .ToList();
    }

    private ScopeCalculationDTO CreateScopeCalculation(
        IGrouping<string, InvoiceLineCalculationDTO> scopeGroup,
        decimal totalEmission,
        IDictionary<Guid, EmissionFactor> emissionFactorLookup)
    {
        var scopeEmission = scopeGroup.Sum(l => l.Emission);
        
        return new ScopeCalculationDTO
        {
            Scope = scopeGroup.Key, // Convert int to string as per DTO
            Emission = scopeEmission,
            Percentage = totalEmission > 0 ? (scopeEmission / totalEmission) * 100 : 0,
            Categories = scopeGroup
                .GroupBy(l => emissionFactorLookup[l.EmissionFactorId!.Value].Category)
                .Select(g => new CategoryCalculationDTO
                {
                    Category = g.Key ?? "Uncategorized", // Handle potential null category
                    Emission = g.Sum(l => l.Emission),
                    Percentage = totalEmission > 0 ? (g.Sum(l => l.Emission) / totalEmission) * 100 : 0
                })
                .ToList()
        };
    }
    
    private IEnumerable<YearlyCalculationDTO> CalculateEmissionsPerYear(
        IEnumerable<InvoiceCalculationDTO> invoiceCalcs,
        IDictionary<Guid, EmissionFactor> emissionFactorLookup)
    {
        return invoiceCalcs
            .GroupBy(i => i.Date.Year.ToString())
            .Select(g => 
            {
                var monthsWithData = g.Select(i => i.Date.Month).Distinct().Count();
                var totalEmission = g.Sum(i => i.Emission);
            
                return new YearlyCalculationDTO
                {
                    Year = g.Key,
                    TotalEmission = totalEmission,
                    AverageMonthlyEmission = monthsWithData > 0 ? totalEmission / monthsWithData : 0,
                    Months = CalculateEmissionsPerMonth(g, emissionFactorLookup)
                };
            });
    }

    private IEnumerable<MonthlyCalculationDTO> CalculateEmissionsPerMonth(
        IGrouping<string, InvoiceCalculationDTO> yearGroup,
        IDictionary<Guid, EmissionFactor> emissionFactorLookup)
    {
        return yearGroup
            .GroupBy(i => i.Date.ToString("MMMM")) // Full month name
            .Select(g => new MonthlyCalculationDTO
            {
                Month = g.Key,
                Emission = g.Sum(i => i.Emission),
                Categories = CalculateCategoriesPerMonth(g, emissionFactorLookup)
            });
    }

    private IEnumerable<CategoryEmissionDTO> CalculateCategoriesPerMonth(
        IGrouping<string, InvoiceCalculationDTO> monthGroup,
        IDictionary<Guid, EmissionFactor> emissionFactorLookup)
    {
        return monthGroup
            .SelectMany(i => i.Lines)
            .GroupBy(l => emissionFactorLookup[l.EmissionFactorId!.Value].Category)
            .Select(g => new CategoryEmissionDTO
            {
                Category = g.Key,
                Emission = g.Sum(l => l.Emission)
            });
    }
}