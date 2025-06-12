using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Tests;

public static class NotificationsTestServiceProviderExtensions
{
    public static IServiceCollection AddDependenciesNotUnderTest(this IServiceCollection services)
    {
        var cosmosClient = new CosmosClient(
            "https://localhost:8081",
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            new CosmosClientOptions());

        services
            .AddLogging()
            .AddSingleton<IMemoryCache, MemoryCache>()
            .AddAutoMapper(typeof(CosmosAutoMapperProfile))
            .AddSingleton(cosmosClient)
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IMetricsRecorder, InMemoryMetricsRecorder>()
            .AddSingleton<ITypedDocumentCosmosStore<NotificationConfigurationDocument>,
                TypedDocumentCosmosStore<NotificationConfigurationDocument>>()
            .AddSingleton<ITypedDocumentCosmosStore<RolesDocument>,
                TypedDocumentCosmosStore<RolesDocument>>()
            .AddSingleton<ITypedDocumentCosmosStore<SiteDocument>,
                TypedDocumentCosmosStore<SiteDocument>>()
            .AddSingleton<ISiteStore, SiteStore>()
            .AddSingleton<IRolesStore, RolesStore>()
            .AddSingleton<INotificationConfigurationStore, NotificationConfigurationStore>()
            .AddSingleton<ISiteService, SiteService>()
            .AddSingleton<INotificationConfigurationService, NotificationConfigurationService>();

        return services;
    }
}
