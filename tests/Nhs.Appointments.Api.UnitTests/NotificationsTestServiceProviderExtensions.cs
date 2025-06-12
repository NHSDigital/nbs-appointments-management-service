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
            .AddSingleton(cosmosClient)
            .AddScoped<IMetricsRecorder, InMemoryMetricsRecorder>()
            .AddAutoMapper(typeof(CosmosAutoMapperProfile))
            .AddTransient<ITypedDocumentCosmosStore<NotificationConfigurationDocument>,
                TypedDocumentCosmosStore<NotificationConfigurationDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<RolesDocument>,
                TypedDocumentCosmosStore<RolesDocument>>()
            .AddTransient<ITypedDocumentCosmosStore<SiteDocument>,
                TypedDocumentCosmosStore<SiteDocument>>()
            .AddSingleton(TimeProvider.System)
            .AddTransient<ISiteService, SiteService>()
            .AddTransient<ISiteStore, SiteStore>()
            .AddTransient<IRolesStore, RolesStore>()
            .AddTransient<INotificationConfigurationStore, NotificationConfigurationStore>()
            .AddTransient<INotificationConfigurationService, NotificationConfigurationService>()
            .AddSingleton<IMemoryCache, MemoryCache>();

        return services;
    }
}
