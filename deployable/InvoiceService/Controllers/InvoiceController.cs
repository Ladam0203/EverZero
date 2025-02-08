using InvoiceService.Core;
using InvoiceService.Core.DTOs;
using InvoiceService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceService.Controllers;

[Route("invoice")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _service;
    
    private readonly RequestContext _requestContext;
    
    public InvoiceController(IInvoiceService service, RequestContext requestContext)
    {
        _service = service;
        _requestContext = requestContext;
    }
    
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        var invoices = await _service.GetInvoicesByUserId((Guid) userId);
        
        //TODO: Remove this block when the service is implemented
        if (!invoices.Any()) {
            // Return a test invoice
            var invoiceId = Guid.NewGuid();
            return Ok(new List<GetInvoiceDTO> {
                new () {
                    Id = invoiceId,
                    UserId = (Guid) userId,
                    Subject = "Test Invoice",
                    SupplierName = "Test Supplier",
                    BuyerName = "Test Buyer",
                    Date = DateTime.Now,
                    
                    Lines = new List<GetInvoiceLineDTO> {
                        new () {
                            Id = Guid.NewGuid(),
                            InvoiceId = invoiceId,
                            Description = "Test Description",
                            Quantity = 1,
                            Unit = "Test Unit"
                        }
                    }
                }
            });
        }
        
        return Ok(invoices);
    }
}