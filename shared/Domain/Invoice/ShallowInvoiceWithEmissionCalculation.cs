namespace Domain;

public class ShallowInvoiceWithEmissionCalculation
{
    public Guid Id { get; set; }
    public string Subject { get; set; }
    public string SupplierName { get; set; }
    public string BuyerName { get; set; }
    public DateTime Date { get; set; }
    
    public Guid UserId { get; set; }

    public List<ShallowInvoiceLineWithEmissionCalculation> Lines { get; set; } = new();
    
    public decimal TotalEmission { get; set; }
}