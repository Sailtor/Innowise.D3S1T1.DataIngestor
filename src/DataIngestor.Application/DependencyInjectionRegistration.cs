using DataIngestor.Application.Interfaces;
using DataIngestor.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataIngestor.Application;

public static class DependencyInjectionRegistration
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMetricsIngestionService, MetricsIngestionService>();
    }
}