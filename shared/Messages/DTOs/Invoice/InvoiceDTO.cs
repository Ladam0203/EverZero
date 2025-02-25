namespace Domain;

public class InvoiceDTO
{
    public Guid Id { get; set; }
    public string Subject { get; set; }
    public string SupplierName { get; set; }
    public string BuyerName { get; set; }
    public DateTime Date { get; set; }
    
    public Guid UserId { get; set; }

    public List<InvoiceLineDTO> Lines { get; set; } = new();
}