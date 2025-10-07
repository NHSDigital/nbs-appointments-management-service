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
        services.AddTransient<INotifyInfoReader<BookingInfo>, BookingInfoReader>();
        services.AddHostedService<ParquetToCsvWorker>();

        services.AddAzureBlobStorage(configuration);
        
        return services;
    }
}
