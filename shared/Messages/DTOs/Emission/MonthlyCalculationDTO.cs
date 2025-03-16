namespace Messages.DTOs.Emission;

public class MonthlyCalculationDTO
{
    public string Month { get; set; }
    public decimal Emission { get; set; } = 0;
    public IEnumerable<CategoryEmissionDTO> Categories { get; set; }
}