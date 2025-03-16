namespace Messages.DTOs.Emission;

public class YearlyCalculationDTO
{
    public string Year { get; set; }
    public decimal TotalEmission { get; set; }
    public decimal AverageMonthlyEmission { get; set; }
    public IEnumerable<MonthlyCalculationDTO> Months { get; set; }
}