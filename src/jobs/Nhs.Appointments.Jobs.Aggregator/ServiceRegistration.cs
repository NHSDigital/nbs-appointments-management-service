using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Json;
using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Jobs.Aggregator;

public static class ServiceRegistration
{
    public static IServiceCollection AddCosmos(this IServiceCollection services, IConfiguration configuration)
    {
        var cosmosEndpoint = configuration["COSMOS_ENDPOINT"];
        var cosmosToken = configuration["COSMOS_TOKEN"];

        CosmosClientOptions options = new()
        {
            HttpClientFactory = () => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }),
            Serializer = new CosmosJsonSerializer(),
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true,
            MaxRetryAttemptsOnRateLimitedRequests = 0
        };

        var cosmos = new CosmosClient(
            accountEndpoint: cosmosEndpoint,
            authKeyOrResourceToken: cosmosToken,
            clientOptions: options);

        services
            .Configure<CosmosDataStoreOptions>(opts => opts.DatabaseName = "appts")
            .AddSingleton(cosmos);
        return services;
    }

    public static IServiceCollection AddAggregationDomain(this IServiceCollection services, string applicationName)
    {
        return services
            .AddTransient<ISiteSummaryAggregator, SiteSummaryAggregator>()
            .AddTransient<IBookingAvailabilityStateService, BookingAvailabilityStateService>()
            .AddTransient<IAvailabilityQueryService, AvailabilityQueryService>()
            .AddTransient<IBookingQueryService, BookingQueryService>()
            .AddTransient<IAvailabilityStore, AvailabilityDocumentStore>()
            .AddTransient<IAvailabilityCreatedEventStore, AvailabilityCreatedEventDocumentStore>()
            .AddTransient<IBookingsDocumentStore, BookingCosmosDocumentStore>()
            .AddTransient<IDailySiteSummaryStore, DailySiteSummaryStore>()
            .AddTransient<ITypedDocumentCosmosStore<AvailabilityCreatedEventDocument>, TypedDocumentCosmosStore<AvailabilityCreatedEventDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<DailyAvailabilityDocument>, TypedDocumentCosmosStore<DailyAvailabilityDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<BookingDocument>, TypedDocumentCosmosStore<BookingDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<BookingIndexDocument>, TypedDocumentCosmosStore<BookingIndexDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<DailySiteSummaryDocument>, TypedDocumentCosmosStore<DailySiteSummaryDocument>>()
            .AddSingleton<ILastUpdatedByResolver>(new LastUpdatedByResolver(applicationName))
            .AddTransient<IMetricsRecorder, InMemoryMetricsRecorder>()
            .AddTransient<IBookingQueryService, BookingQueryService>()
            .AddAutoMapper(typeof(CosmosAutoMapperProfile));
    }
}
