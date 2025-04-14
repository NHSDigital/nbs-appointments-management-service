using Nhs.Appointments.Persistance.Models;
using Nhs.Appointments.Persistance;
using System.Reflection;

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
}
