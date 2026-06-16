using DataIngestor.Application.Interfaces;
using DataIngestor.Presentation.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly.CircuitBreaker;

namespace DataIngestor.Presentation.Tests.Controllers;

public class APITestControllerTests
{
    private readonly IMetricsIngestionService ingestionService = Substitute.For<IMetricsIngestionService>();
    private readonly APITestController sut;

    public APITestControllerTests()
    {
        sut = new APITestController(ingestionService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
    }

    [Fact]
    public async Task GetReadingsWhenIngestionSucceedsReturns200()
    {
        // Arrange
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await sut.GetReadings(CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetReadingsWhenIngestionSucceedsCallsIngestExactlyOnce()
    {
        // Arrange
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await sut.GetReadings(CancellationToken.None);

        // Assert
        await ingestionService.Received(1).IngestAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReadingsWhenCircuitBrokenWithRetryAfterReturns503WithRetryAfterHeader()
    {
        // Arrange
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new BrokenCircuitException("Circuit open", TimeSpan.FromSeconds(30)));

        // Act
        IActionResult result = await sut.GetReadings(CancellationToken.None);

        // Assert
        ObjectResult problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, problem.StatusCode);

        string? retryAfterHeader = sut.HttpContext.Response.Headers["Retry-After"].FirstOrDefault();
        Assert.Equal("30", retryAfterHeader);
    }

    [Fact]
    public async Task GetReadingsWhenCircuitBrokenWithoutRetryAfterReturns503WithoutRetryAfterHeader()
    {
        // Arrange
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new BrokenCircuitException("Circuit open"));

        // Act
        IActionResult result = await sut.GetReadings(CancellationToken.None);

        // Assert
        ObjectResult problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, problem.StatusCode);
        Assert.False(sut.HttpContext.Response.Headers.ContainsKey("Retry-After"));
    }

    [Fact]
    public async Task GetReadingsWhenCircuitBrokenProblemDetailDescribesUnavailability()
    {
        // Arrange
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new BrokenCircuitException("open"));

        // Act
        IActionResult result = await sut.GetReadings(CancellationToken.None);

        // Assert
        ObjectResult problem = Assert.IsType<ObjectResult>(result);
        ProblemDetails details = Assert.IsType<ProblemDetails>(problem.Value);
        Assert.Equal("Service Unavailable", details.Title);
        Assert.Contains("unavailable", details.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetReadingsWhenUnexpectedExceptionThrownReturns500()
    {
        // Arrange
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("upstream exploded"));

        // Act
        IActionResult result = await sut.GetReadings(CancellationToken.None);

        // Assert
        ObjectResult problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }

    [Fact]
    public async Task GetReadingsWhenUnexpectedExceptionThrownProblemDetailContainsExceptionMessage()
    {
        // Arrange
        const string errorMsg = "something went wrong";
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(errorMsg));

        // Act
        IActionResult result = await sut.GetReadings(CancellationToken.None);

        // Assert
        ObjectResult problem = Assert.IsType<ObjectResult>(result);
        ProblemDetails details = Assert.IsType<ProblemDetails>(problem.Value);
        Assert.Equal(errorMsg, details.Detail);
    }

    [Fact]
    public async Task GetReadingsPassesCancellationTokenToIngestionService()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        ingestionService.IngestAsync(cts.Token).Returns(Task.CompletedTask);

        // Act
        await sut.GetReadings(cts.Token);

        // Assert
        await ingestionService.Received(1).IngestAsync(cts.Token);
    }
}
