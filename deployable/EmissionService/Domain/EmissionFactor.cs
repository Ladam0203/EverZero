using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EmissionService.Domain;

public class EmissionFactor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EmissionFactorMetadata EmissionFactorMetadata { get; set; }
    public string Category { get; set; } // This is called activity in the UK conversion factors
    public Dictionary<string, string> SubCategories { get; set; }
    public string Unit { get; set; }
    public decimal CarbonEmissionKg { get; set; }
}