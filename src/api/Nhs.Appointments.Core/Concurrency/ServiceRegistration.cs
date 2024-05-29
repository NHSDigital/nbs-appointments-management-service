using Nhs.Appointments.Core.Concurrency;
using Microsoft.Extensions.Azure;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInMemoryLeasing(this IServiceCollection services)
    {
        services.Configure<SiteLeaseManagerOptions>(opts => opts.Timeout = TimeSpan.FromSeconds(15));
        return services.AddSingleton<ISiteLeaseManager, InMemorySiteLeaseManager>();
    }

    public static IServiceCollection AddAzureBlobStoreLeasing(this IServiceCollection services, string connectionString, string containerName)
    {
        services.Configure<SiteLeaseManagerOptions>(opts => { 
            opts.Timeout = TimeSpan.FromSeconds(30);
            opts.ContainerName = containerName;
        });

        services.AddAzureClients(x =>
        {
            x.AddBlobServiceClient(connectionString);
        });

        return services.AddSingleton<ISiteLeaseManager, AzureStorageSiteLeaseManager>();
    }
}
