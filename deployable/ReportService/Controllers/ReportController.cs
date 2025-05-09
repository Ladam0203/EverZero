using Context;
using Domain.Emission;
using Messages.DTOs.Report;
using Microsoft.AspNetCore.Mvc;
using ReportService.Services;

namespace ReportService.Controllers;

[ApiController]
[Route("report")]
public class ReportController : ControllerBase
{
    private readonly IReportService _service;
    
    private readonly RequestContext _requestContext;
    
    public ReportController(IReportService service, RequestContext requestContext)
    {
        _service = service;
        _requestContext = requestContext;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] PostReportDTO dto)
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        return Ok(await _service.Create((Guid) userId, dto));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllReports()
    {
        var userId = _requestContext.UserId;
        if (userId is null) {
            return Unauthorized("User not authenticated or authorized");
        }
        
        return Ok(await _service.GetAllByUserId((Guid) userId));
    }
}