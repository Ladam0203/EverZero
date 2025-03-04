namespace Messages.DTOs.Emission;

public class ScopeCalculationDTO
{
    public string Scope { get; set; }
    public decimal Emission { get; set; }
    public decimal Percentage { get; set; }
    
    public IEnumerable<CategoryCalculationDTO> Categories { get; set; }
}