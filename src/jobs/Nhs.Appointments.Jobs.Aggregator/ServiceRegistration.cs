using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core.Json;

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

        services.AddSingleton(cosmos);
        return services;
    }
}
