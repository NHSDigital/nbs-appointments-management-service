using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Json;
using Nhs.Appointments.Core.Metrics;
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
            .AddScoped<ISiteSummaryAggregator, SiteSummaryAggregator>()
            .AddScoped<IBookingAvailabilityStateService, BookingAvailabilityStateService>()
            .AddScoped<IAvailabilityQueryService, AvailabilityQueryService>()
            .AddScoped<IBookingQueryService, BookingQueryService>()
            .AddScoped<IAvailabilityStore, AvailabilityDocumentStore>()
            .AddScoped<IAvailabilityCreatedEventStore, AvailabilityCreatedEventDocumentStore>()
            .AddScoped<IBookingsDocumentStore, BookingCosmosDocumentStore>()
            .AddScoped<IDailySiteSummaryStore, DailySiteSummaryStore>()
            .AddScoped<ITypedDocumentCosmosStore<AvailabilityCreatedEventDocument>, TypedDocumentCosmosStore<AvailabilityCreatedEventDocument>>()
            .AddScoped<ITypedDocumentCosmosStore<DailyAvailabilityDocument>, TypedDocumentCosmosStore<DailyAvailabilityDocument>>()
            .AddScoped<ITypedDocumentCosmosStore<BookingDocument>, TypedDocumentCosmosStore<BookingDocument>>()
            .AddScoped<ITypedDocumentCosmosStore<BookingIndexDocument>, TypedDocumentCosmosStore<BookingIndexDocument>>()
            .AddScoped<ITypedDocumentCosmosStore<DailySiteSummaryDocument>, TypedDocumentCosmosStore<DailySiteSummaryDocument>>()
            .AddSingleton<ILastUpdatedByResolver>(new LastUpdatedByResolver(applicationName))
            .AddScoped<IMetricsRecorder, InMemoryMetricsRecorder>()
            .AddScoped<IBookingQueryService, BookingQueryService>()
            .AddAutoMapper(typeof(CosmosAutoMapperProfile));
    }
}
