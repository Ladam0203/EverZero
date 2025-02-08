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
        return Ok(invoices);
    }
}