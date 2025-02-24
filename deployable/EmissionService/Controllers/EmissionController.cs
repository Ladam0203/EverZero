using EmissionService.Domain;
using EmissionService.Domain.DTOs;
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
    public async Task<IActionResult> CalculateEmission([FromBody] EmissionCalculationRequest request)
    {
        var result = await _service.CalculateEmission(request);
        return Ok(result);
    }
}