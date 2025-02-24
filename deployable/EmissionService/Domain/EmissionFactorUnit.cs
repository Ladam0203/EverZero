namespace EmissionService.Domain;

public class EmissionFactorUnit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Unit { get; set; }
    public decimal CarbonEmissionKg { get; set; }
}