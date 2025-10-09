using DataExtract;
using DataExtract.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Persistance.Models;

namespace JobRunner.Job.BookingExtract;

public static class ServiceRegistration
{
    public static IServiceCollection AddBookingExtractJob(this IServiceCollection services,
        IConfigurationBuilder configurationBuilder)
    {
        services.AddDataExtractServices("booking", configurationBuilder)
            .AddCosmosStore<NbsBookingDocument>()
            .AddExtractWorker<BookingDataExtract>()
            .Configure<BookingQueryOptions>(config =>
            {
                var configuration = configurationBuilder.Build();
                
                config.Services = configuration.GetValue<string>("BookingQueryServices")?.Split(',') ??
                                  throw new ArgumentNullException("BookingQueryServices");
                config.From = configuration.GetValue<DateOnly?>("BookingQueryFrom") ?? throw new ArgumentNullException("BookingQueryFrom");
                config.To = configuration.GetValue<DateOnly?>("BookingQueryTo") ?? throw new ArgumentNullException("BookingQueryTo");
            });
        
        return services;
    }
}
