using Nhs.Appointments.Persistance.Models;
using Nhs.Appointments.Persistance;
using System.Reflection;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddCosmosDataStores(this IServiceCollection services)
    {
        var documentTypes = typeof(TypedCosmosDocument).Assembly
            .GetTypes()
            .Where(t => typeof(TypedCosmosDocument).IsAssignableFrom(t) && t.GetCustomAttribute<CosmosDocumentTypeAttribute>() != null);

        foreach (var documentType in documentTypes)
        {
            var serviceType = typeof(ITypedDocumentCosmosStore<>).MakeGenericType(documentType);
            var implementationType = typeof(TypedDocumentCosmosStore<>).MakeGenericType(documentType);
            services.AddTransient(serviceType, implementationType);
        }

        return services;
    }

    public static IServiceCollection AddEulaBlobStore(this IServiceCollection services)
    {
        var eulaBlobStoreConnectionString = Environment.GetEnvironmentVariable("EULA_BLOB_STORE__CONNECTION_STRING");
        var eulaBlobStoreContainerName = Environment.GetEnvironmentVariable("EULA_BLOB_STORE__CONTAINER_NAME");
        if (string.IsNullOrWhiteSpace(eulaBlobStoreConnectionString)
            || string.IsNullOrWhiteSpace(eulaBlobStoreContainerName))
        {
            throw new NullReferenceException("Could not configure eula blob storage service. Connection string or container name was missing.");
        }

        services.Configure<EulaStoreOptions>(opts => {
            opts.ConnectionString = eulaBlobStoreConnectionString;
            opts.ContainerName = eulaBlobStoreContainerName;
        });

        return services.AddSingleton<IEulaStore, EulaStore>();
    }
}
