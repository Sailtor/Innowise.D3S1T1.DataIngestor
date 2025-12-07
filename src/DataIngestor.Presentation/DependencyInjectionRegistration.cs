using DataIngestor.Presentation.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Impl.Matchers;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Security.Principal;

namespace DataIngestor.Presentation;

public static class DependencyInjectionRegistration
{
    public static void AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
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
            // handy when part of cluster or you want to otherwise identify multiple schedulers
            q.SchedulerId = "Scheduler-Core";
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });

            // quickest way to create a job with single trigger is to use ScheduleJob
            q.ScheduleJob<MetricReaderJob>(trigger => trigger
                .WithIdentity("10 second cron Trigger")
                .StartNow()
                .WithCronSchedule("0/3 * * * * ?")
                .WithDescription("10 second cron trigger")
            );
        });
    }
}