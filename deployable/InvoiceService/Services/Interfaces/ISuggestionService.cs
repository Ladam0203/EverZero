using Domain.Suggestions;

namespace InvoiceService.Services.Interfaces;

public interface ISuggestionService
{
    Task<EmissionFactorIdSuggestionDTO?> GetEmissionFactorIdSuggestion(string supplierName, string invoiceLineDescription, string unit);
}