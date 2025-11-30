using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataIngestor.Presentation;

public static class DependencyInjectionRegistration
{
    public static void AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddControllers(services);
    }

    private static void AddControllers(IServiceCollection services)
    {
        services.AddControllers();
    }
}