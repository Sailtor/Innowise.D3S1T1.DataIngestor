using DataIngestor.Presentation.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DataIngestor.Presentation;

public static class DependencyInjectionRegistration
{
    public static void AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        AddControllers(services);
        AddQuartzJobs(services, configuration);
    }

    private static void AddControllers(IServiceCollection services)
    {
        services.AddControllers();
    }

    private static void AddQuartzJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuartzOptions>(configuration.GetSection("Quartz"));
        services.Configure<QuartzOptions>(options =>
        {
            options.Scheduling.IgnoreDuplicates = true;
            options.Scheduling.OverWriteExistingData = false;
        });

        services.AddQuartz(q =>
        {
            q.SchedulerId = "Scheduler-Core";
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });

            q.ScheduleJob<MetricReaderJob>(trigger => trigger
                .WithIdentity("10 second cron Trigger")
                .StartNow()
                .WithCronSchedule("0/10 * * * * ?")
                .WithDescription("10 second cron trigger")
            );
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    }
}