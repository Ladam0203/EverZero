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
    
    public EmissionController(IEmissionFactorService service)
    {
        _service = service;
    }
    
    [HttpGet("emission-factors")]
    public async Task<IEnumerable<EmissionFactor>> GetAll()
    {
        return await _service.GetAll();
    }
    
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateEmission([FromBody] IEnumerable<InvoiceDTO> invoices)
    {
        var result = await _service.CalculateEmission(invoices);
        return Ok(result);
    }
}