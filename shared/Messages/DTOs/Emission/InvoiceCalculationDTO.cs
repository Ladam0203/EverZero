namespace Domain;

public class InvoiceCalculationDTO
{
    public Guid Id { get; set; }
    public string Subject { get; set; }
    public string SupplierName { get; set; }
    public string BuyerName { get; set; }
    public DateTime Date { get; set; }
    
    public Guid UserId { get; set; }

    public List<InvoiceLineCalculationDTO> Lines { get; set; } = new();
    
    public decimal Emission { get; set; } = 0;
}