using Context;
using InvoiceService.Core.DTOs;
using InvoiceService.Services;
using InvoiceService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceService.Controllers;

[Route("invoice")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly ISuggestionService _suggestion;
    
    private readonly RequestContext _requestContext;
    
    public InvoiceController(IInvoiceService service, 
        ISuggestionService suggestion,
        RequestContext requestContext)
    {
        _service = service;
        _suggestion = suggestion;
        _requestContext = requestContext;
    }
    
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        // Adjust startDate to the beginning of the day if provided
        startDate = startDate?.Date ?? DateTime.MinValue;

        // Adjust endDate to the end of the day if provided
        endDate = endDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        try
        {
            var invoices = await _service.GetAllByUserId((Guid) userId, (DateTime) startDate, (DateTime) endDate);
            return Ok(invoices);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpPost("invoices")]
    public async Task<IActionResult> PostInvoice([FromBody] PostInvoiceDTO dto)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        var response = await _service.Create((Guid) userId, dto);
        
        return Ok(response);
    }
    
    [HttpGet("suggestions/emission-factor-id")]
    public async Task<IActionResult> SuggestEmissionFactorId([FromQuery] string supplierName, [FromQuery] string invoiceLineDescription, [FromQuery] string unit)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        var suggestion = await _suggestion.GetEmissionFactorIdSuggestion(supplierName, invoiceLineDescription, unit);
        
        if (suggestion is null) {
            return NotFound();
        }
        
        return Ok(suggestion);
    }
    
    [HttpDelete("invoices/{invoiceId}")]
    public async Task<IActionResult> DeleteInvoice(Guid invoiceId)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }

        try
        {
            await _service.Delete((Guid) userId, invoiceId);
        } 
        catch (KeyNotFoundException e)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException e)
        {
            return NotFound(); // Security through obscurity
        }

        return NoContent();
    }
}