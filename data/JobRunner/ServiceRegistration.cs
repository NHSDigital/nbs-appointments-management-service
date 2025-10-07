using JobRunner.Job.BookingExtract;
using JobRunner.Job.Notify;
using JobRunner.Job.ParquetToCsv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobRunner;

public static class ServiceRegistration
{
    public static IServiceCollection AddJob(this IServiceCollection services,
        IConfigurationBuilder configurationBuilder)
    {
        var configuration = configurationBuilder.Build();
        var job = configuration.GetValue<string>("Job");

        switch (job)
        {
            case "BookingExtract":
                services.AddBookingExtractJob(configurationBuilder);
                break;
            case "Notify":
                services.AddNotifyJob(configurationBuilder);
                break;
            case "ParquetToCSV":
                services.AddParquetToCSV(configurationBuilder);
                break;
            default:
                throw new InvalidOperationException($"Job {job} is not recognized");
        }
        
        return services;
    }
}
