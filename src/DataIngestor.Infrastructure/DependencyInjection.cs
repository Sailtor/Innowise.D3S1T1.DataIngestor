using DataIngestor.Application.Interfaces;
using DataIngestor.Infrastructure.APIClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataIngestor.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddHttpClients(services, configuration);
        AddAppServices(services);
    }

    private static void AddHttpClients(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("WeakAPIClient", client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection("Integrations:WeakAPI:URL").Value);
            client.DefaultRequestHeaders.Add("X-Api-Key", configuration.GetSection("Integrations:WeakAPI:ApiKey").Value);
        });
    }
    private static void AddAppServices(IServiceCollection services)
    {
        services.AddScoped<IMetricReader, WeakAPIMetricReader>();
    }
}