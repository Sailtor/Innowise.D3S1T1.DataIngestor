using DataIngestor.Application.Interfaces;
using DataIngestor.Contracts;
using DataIngestor.Infrastructure.APIClients;
using DataIngestor.Infrastructure.Messaging;
using DataIngestor.Infrastructure.Options;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DataIngestor.Infrastructure;

public static class DependencyInjectionRegistration
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddHttpClients(services, configuration);
        AddAppServices(services);
        AddMessaging(services, configuration);
    }

    private static void AddHttpClients(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WeakApiOptions>(configuration.GetSection(WeakApiOptions.SectionName));

        services.AddHttpClient<IMetricReader, WeakAPIMetricReader>((serviceProvider, client) =>
        {
            WeakApiOptions options = serviceProvider
                .GetRequiredService<IOptions<WeakApiOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.Url);
            client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
        }).AddStandardResilienceHandler();

        string resilienceOptionsName = $"{typeof(IMetricReader).FullName}-standard";
        services.AddOptions<HttpStandardResilienceOptions>(resilienceOptionsName)
            .Bind(configuration.GetSection(WeakApiOptions.ResilienceSectionName));
    }

    private static void AddAppServices(IServiceCollection services)
    {
        services.AddScoped<IMetricPublisher, MetricPublisher>();
        services.AddAutoMapper(AssemblyReference.Assembly);
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                RabbitMqOptions options = ctx
                    .GetRequiredService<IOptions<RabbitMqOptions>>()
                    .Value;

                cfg.Host(options.Host, options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });

                cfg.Message<MetricReadingsBatch>(x => x.SetEntityName("metric-readings"));
                cfg.Publish<MetricReadingsBatch>(x => x.ExchangeType = ExchangeType.Fanout);
            });
        });
    }
}
