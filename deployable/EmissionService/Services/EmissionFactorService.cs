using Domain;
using Domain.Emission;
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
    
    public async Task<EmissionCalculation> CalculateEmission(EmissionCalculationRequest request)
    {
        // Fetch the emission factors present in the invoices
        var emissionFactorIds = request.Invoices
            .SelectMany(i => i.Lines
                .Select(l => l.EmissionFactorId)
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id.Value)) // We are ignoring the null and empty values TODO: Raise an error
            .ToList();
        
        var emissionFactors = await _repository.GetByIds(emissionFactorIds);
        

        
        var emissionCalculation = new EmissionCalculation();
        
        // Add & map invoices to the emission calculation
        emissionCalculation.Invoices = request.Invoices.Select(i => new ShallowInvoiceWithEmissionCalculation()
        {
            Id = i.Id,
            Subject = i.Subject,
            SupplierName = i.SupplierName,
            BuyerName = i.BuyerName,
            Date = i.Date,
            UserId = i.UserId,
            Lines = i.Lines.Select(l => new ShallowInvoiceLineWithEmissionCalculation()
            {
                Id = l.Id,
                Description = l.Description,
                Quantity = l.Quantity,
                Unit = l.Unit,
                EmissionFactorId = l.EmissionFactorId,
                EmissionFactorUnitId = l.EmissionFactorUnitId
            }).ToList()
        }).ToList();

        // Calculate the total emission
        var invoices = emissionCalculation.Invoices;
        
        foreach (var invoice in invoices)
        {
            foreach (var line in invoice.Lines)
            {
                // Find the emission factor
                var emissionFactor = emissionFactors.FirstOrDefault(ef => ef.Id == line.EmissionFactorId);
                
                //Find the conversion factor
                var conversionFactor = emissionFactor.EmissionFactorUnit.FirstOrDefault(u => u.Id == line.EmissionFactorUnitId);
                
                if (emissionFactor == null || conversionFactor == null) // TODO: Raise an error
                {
                    continue;
                }
                
                line.TotalEmission = line.Quantity * conversionFactor.CarbonEmissionKg;
            }
            
            invoice.TotalEmission = invoice.Lines.Sum(l => l.TotalEmission);
        }
        emissionCalculation.TotalEmission = invoices.Sum(i => i.TotalEmission);
        
        return emissionCalculation;
    }
}