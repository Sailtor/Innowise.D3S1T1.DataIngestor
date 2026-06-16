using AutoMapper;
using DataIngestor.Application.Interfaces;
using DataIngestor.Domain.Messages;
using DataIngestor.Infrastructure.APIClients;
using DataIngestor.Infrastructure.Messaging;
using MassTransit;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace DataIngestor.Infrastructure;

public static class DependencyInjectionRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddHttpClients(services, configuration);
        AddAppServices(services);
        AddMessaging(services, configuration);
    }

    private static void AddHttpClients(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("WeakAPIClient", client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection("Integrations:WeakAPI:URL").Value);
            client.DefaultRequestHeaders.Add("X-Api-Key", configuration.GetSection("Integrations:WeakAPI:ApiKey").Value);
        }).AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);

            options.Retry.MaxRetryAttempts = 5;
            options.Retry.BackoffType = DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
            options.Retry.Delay = TimeSpan.FromSeconds(2);

            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);

            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
        });
    }

    private static void AddAppServices(IServiceCollection services)
    {
        services.AddScoped<IMetricReader, WeakAPIMetricReader>();
        services.AddScoped<IMetricPublisher, MetricPublisher>();
        services.AddAutoMapper(typeof(MetricReadingProfile));
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration.GetSection("RabbitMQ:Host").Value, configuration.GetSection("RabbitMQ:VirtualHost").Value, h =>
                {
                    h.Username(configuration.GetSection("RabbitMQ:Username").Value!);
                    h.Password(configuration.GetSection("RabbitMQ:Password").Value!);
                });

                cfg.Message<MetricReadingsBatch>(x => x.SetEntityName("metric-readings"));
                cfg.Publish<MetricReadingsBatch>(x => x.ExchangeType = ExchangeType.Fanout);
            });
        });
    }
}