namespace EmissionService.Domain;

public class FactorSource
{
    // This is really specific to the UK conversion factors, in the future, this could be also a key-value pair
    public string EmissionSource { get; set; }
    public string Scope { get; set; }
    public DateTime NextPublicationDate { get; set; }
    public string Version { get; set; }
    public string FactorSet { get; set; }
    public string Year { get; set; }
}