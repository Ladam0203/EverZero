using Domain.Suggestions;
using InvoiceService.Repository;
using InvoiceService.Services.Interfaces;

namespace InvoiceService.Services;

public class SuggestionService : ISuggestionService
{
    private readonly IInvoiceRepository _repository;

    public SuggestionService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmissionFactorIdSuggestionDTO?> GetEmissionFactorIdSuggestion(string supplierName,
        string invoiceLineDescription, string unit)
    {
        var emissionFactorId = await _repository.GetEmissionFactorIdBy(supplierName, invoiceLineDescription, unit);
        
        if (emissionFactorId is null) {
            return null;
        }
        
        return new EmissionFactorIdSuggestionDTO() {
            EmissionFactorId = (Guid) emissionFactorId
        };
    }
}