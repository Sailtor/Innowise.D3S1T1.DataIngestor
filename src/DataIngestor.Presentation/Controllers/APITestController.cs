using DataIngestor.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataIngestor.Presentation.Controllers;

[ApiController]
[Route("api/test")]
public class APITestController(IMetricReader metricReader) : ControllerBase
{

    [HttpGet("readings")]
    public async Task<IActionResult> GetReadings(CancellationToken ct)
    {
        try
        {
            var readings = await metricReader.ReadMetricsAsync(ct);

            return Ok(readings);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to fetch from WeakApi"
            );
        }
    }
}