using DataIngestor.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using System.Globalization;

namespace DataIngestor.Presentation.Controllers;

[ApiController]
[Route("api/test")]
public class APITestController(IMetricsIngestionService ingestionService) : ControllerBase
{
    [HttpGet("readings")]
    public async Task<IActionResult> GetReadings(CancellationToken cancellationToken)
    {
        try
        {
            await ingestionService.IngestAsync(cancellationToken);

            return Ok();
        }
        catch (BrokenCircuitException ex)
        {
            if (ex.RetryAfter.HasValue)
            {
                Response.Headers.RetryAfter = ex.RetryAfter.Value.TotalSeconds.ToString("0", CultureInfo.InvariantCulture);
            }

            return Problem(
                detail: "The upstream metrics service is currently unavailable. Please try again later.",
                title: "Service Unavailable",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to fetch from WeakApi: internal server error.");
        }
    }
}
