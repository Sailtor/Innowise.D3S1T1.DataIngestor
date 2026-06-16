using DataIngestor.Application.Interfaces;
using DataIngestor.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Quartz;

namespace DataIngestor.Presentation.Tests.Jobs;

public class MetricReaderJobTests
{
    private static IServiceScopeFactory BuildScopeFactory(IMetricsIngestionService ingestionService)
    {
        ServiceCollection services = new();
        services.AddScoped(_ => ingestionService);
        ServiceProvider sp = services.BuildServiceProvider();
        return sp.GetRequiredService<IServiceScopeFactory>();
    }

    private static IJobExecutionContext StubContext()
    {
        return Substitute.For<IJobExecutionContext>();
    }

    [Fact]
    public async Task ExecuteWhenIngestionSucceedsCallsIngestExactlyOnce()
    {
        // Arrange
        IMetricsIngestionService ingestionService = Substitute.For<IMetricsIngestionService>();
        ingestionService.IngestAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        MetricReaderJob sut = new(
            BuildScopeFactory(ingestionService),
            Substitute.For<ILogger<MetricReaderJob>>());

        // Act
        await sut.Execute(StubContext());

        // Assert
        await ingestionService.Received(1).IngestAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWhenIngestionThrowsDoesNotPropagateException()
    {
        // Arrange
        IMetricsIngestionService ingestionService = Substitute.For<IMetricsIngestionService>();
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("broker unavailable"));

        MetricReaderJob sut = new(
            BuildScopeFactory(ingestionService),
            Substitute.For<ILogger<MetricReaderJob>>());

        // Act & Assert
        await sut.Execute(StubContext());
    }

    [Fact]
    public async Task ExecuteWhenHttpRequestExceptionThrowsDoesNotPropagateException()
    {
        // Arrange
        IMetricsIngestionService ingestionService = Substitute.For<IMetricsIngestionService>();
        ingestionService.IngestAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("connection refused"));

        MetricReaderJob sut = new(
            BuildScopeFactory(ingestionService),
            Substitute.For<ILogger<MetricReaderJob>>());

        // Act & Assert
        await sut.Execute(StubContext());
    }
}
