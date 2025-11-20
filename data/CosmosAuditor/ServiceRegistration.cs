using CosmosAuditor.Blob;
using CosmosAuditor.Containers;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core.Json;

namespace CosmosAuditor;

public static class ServiceRegistration
{
    public static IServiceCollection AddAuditWorker<TConfig>(this IServiceCollection services) where TConfig : ContainerConfig
    {
        services.AddSingleton<TConfig>();
        services.AddHostedService<AuditWorker<TConfig>>();
        
        return services;
    }

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
            LimitToEndpoint = true
        };

        var cosmos = new CosmosClient(
            accountEndpoint: cosmosEndpoint,
            authKeyOrResourceToken: cosmosToken,
            clientOptions: options);
        
        services.AddSingleton(cosmos);
        return services;
    }
    
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services,
        IConfiguration configuration)
    { 
        services
            .AddSingleton<IAzureBlobStorage, AzureBlobStorage>()
            .AddAzureClients(x =>
            {
                x.AddBlobServiceClient(configuration["BLOB_STORAGE_CONNECTION_STRING"]);
            });
        
        return services;
    }
}
