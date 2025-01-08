using BookingsDataExtracts.Documents;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Json;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;
using Nhs.Appointments.Persistance.Models;

namespace BookingsDataExtracts;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataExtractServices(this IServiceCollection services, IConfigurationBuilder configurationBuilder)
    {
        var configuration = configurationBuilder.Build();
        var cosmosEndpoint = configuration["COSMOS_ENDPOINT"];
        var cosmosToken = configuration["COSMOS_TOKEN"];
        var destinationMailbox = configuration["MESH_MAILBOX_DESTINATION"];
        var meshWorkflow = configuration["MESH_WORKFLOW"];

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

        services
            .Configure<CosmosStoreOptions>(opts => opts.DatabaseName = "appts")
            .Configure<MeshSendOptions>(opts =>
            {
                opts.DestinationMailboxId = destinationMailbox;
                opts.WorkflowId = meshWorkflow;
            })
            .AddSingleton<TimeProvider>(TimeProvider.System)
            .AddSingleton(cosmos)
            .AddSingleton<CosmosStore<NbsBookingDocument>>()
            .AddSingleton<CosmosStore<SiteDocument>>()
            .AddSingleton<BookingDataExtract>()
        .AddMesh(configuration);

        var azureKeyVaultConfig = configuration.GetSection("KeyVault")?.Get<AzureKeyVaultConfiguration>();
        if (!string.IsNullOrEmpty(azureKeyVaultConfig?.KeyVaultName))
        {
            services.Configure<AzureKeyVaultConfiguration>(opts =>
            {
                opts.KeyVaultName = azureKeyVaultConfig.KeyVaultName;
                opts.ClientId = azureKeyVaultConfig.ClientId;
                opts.TenantId = azureKeyVaultConfig.TenantId;
                opts.ClientSecret = azureKeyVaultConfig.ClientSecret;
            });
            services.AddSingleton<ICertificateProvider, KeyVaultCertificateProvider>();
        }        

        return services;
    }
}
