using Microsoft.AspNetCore.Mvc;

namespace InvoiceService.Controllers;

[Route("invoice")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly RequestContext _requestContext;
    
    public InvoiceController(RequestContext requestContext)
    {
        _requestContext = requestContext;
    }
    
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        return Ok("Invoices, user id: " + _requestContext.UserId);
        //return Ok(_requestContext.UserId);
    }
}