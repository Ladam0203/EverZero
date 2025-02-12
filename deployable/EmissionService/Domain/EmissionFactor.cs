using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EmissionService.Domain;

public class EmissionFactor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    //public FactorSource FactorSource { get; set; } // TODO: Implement, this is just extra info, not currently used
    public string Category { get; set; } // This is called activity in the UK conversion factors
    public Dictionary<string, string> SubCategories { get; set; }
    public List<UnitEmissionFactor> UnitEmissionFactors { get; set; }
}