using Context;
using InvoiceService.Core.DTOs;
using InvoiceService.Services;
using InvoiceService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace InvoiceService.Controllers;

[Route("invoice")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly ISuggestionService _suggestion;
    
    private readonly RequestContext _requestContext;
    
    private readonly ILogger _logger;
    
    public InvoiceController(IInvoiceService service, 
        ISuggestionService suggestion,
        RequestContext requestContext,
        ILogger logger)
    {
        _service = service;
        _suggestion = suggestion;
        _requestContext = requestContext;
        _logger = logger;
    }
    
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        // Adjust startDate to the beginning of the day if provided, otherwise beginning of year
        startDate = startDate?.Date ?? new DateTime(DateTime.Now.Year, 1, 1);

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
    
    // Bulk Post
    [HttpPost("invoices/bulk")]
    public async Task<IActionResult> PostInvoices([FromBody] IEnumerable<PostInvoiceDTO> dtos)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        var response = await _service.CreateAll((Guid) userId, dtos);
        
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
    
    [HttpPut("invoices/{invoiceId}")]
    public async Task<IActionResult> UpdateInvoice(Guid invoiceId, [FromBody] PutInvoiceDTO dto)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        if (invoiceId != dto.Id) {
            return BadRequest("Invoice ID in URL does not match ID in body");
        }

        try
        {
            var invoice = await _service.Update((Guid) userId, dto);
            return Ok(invoice);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException e)
        {
            return NotFound(); // Security through obscurity
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating invoice");
            return StatusCode(500, e.Message);
        }
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