namespace Domain;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Path { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalEmission { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}