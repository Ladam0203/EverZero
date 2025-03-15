using Context;
using Domain;
using EmissionService.Domain;
using EmissionService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmissionService.Controllers;

[Route("emission")]
[ApiController]
public class EmissionController : ControllerBase
{
    private readonly IEmissionFactorService _service;
    private readonly ICalculationService _calculation;
    
    private readonly RequestContext _requestContext;
    
    public EmissionController(IEmissionFactorService service, 
        ICalculationService calculation,
        RequestContext requestContext)
    {
        _service = service;
        _calculation = calculation;
        _requestContext = requestContext;
    }
    
    [HttpGet("emission-factors")]
    public async Task<IEnumerable<EmissionFactor>> GetAll()
    {
        return await _service.GetAll();
    }
    
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateEmission([FromBody] IEnumerable<InvoiceDTO> invoices)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        var result = await _calculation.CalculateEmission((Guid) userId, invoices);
        return Ok(result);
    }
}