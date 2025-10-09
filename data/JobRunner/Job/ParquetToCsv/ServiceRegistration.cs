using JobRunner.Job.Notify;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobRunner.Job.ParquetToCsv;

public static class ServiceRegistration
{
    public static IServiceCollection AddParquetToCSV(this IServiceCollection services,
        IConfigurationBuilder configurationBuilder)
    {
        var configuration = configurationBuilder.Build();
        
        services
            .Configure<JobOptions>(config =>
                {
                    config.Environment = configuration.GetValue<string>("Environment") ??
                                         throw new NullReferenceException("Configuration for Environment is missing");
                    config.Notification = configuration.GetValue<string>("Notification") ??
                                         throw new NullReferenceException("Configuration for Notification is missing");
                })
            .AddTransient<INotifyInfoReader<BookingInfo>, BookingInfoReader>()
            .AddHostedService<ParquetToCsvWorker>()
            .AddTransient<ISendTracker, BlobSendTracker>()
            .AddAzureBlobStorage(configuration);
        
        return services;
    }
}
