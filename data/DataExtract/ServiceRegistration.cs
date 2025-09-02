using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;
using Nhs.Appointments.Api.Json;

namespace DataExtract;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataExtractServices(this IServiceCollection services, string fileName,
        IConfigurationBuilder configurationBuilder)
    {
        var configuration = configurationBuilder.Build();
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

        services
            .Configure<CosmosStoreOptions>(opts => opts.DatabaseName = "appts")
            .Configure<FileOptions>(opts =>
            {
                opts.FileName = fileName;
            })
            .AddSingleton(TimeProvider.System)
            .AddSingleton(cosmos)
            .AddFileSender(configuration)
            .AddMesh(configuration);



        return services;
    }

    public static IServiceCollection AddCosmosStore<TDocument>(this IServiceCollection services) where TDocument : class
    { 
        services.AddSingleton<CosmosStore<TDocument>>();

        return services;
    }

    public static IConfigurationBuilder AddNbsAzureKeyVault(this IConfigurationBuilder builder, string configSectionName = AzureKeyVaultConfiguration.DefaultConfigSectionName)
    {
        var tempConfig = builder.Build();
        var config = tempConfig.GetAzureKeyVaultConfiguration(configSectionName);

        if (config is null || string.IsNullOrEmpty(config?.KeyVaultName))
        {
            Console.WriteLine("Failed to add key vault config");
            return builder;
        }

        Console.WriteLine("Added key vault config");
        var keyVaultUri = new Uri($"https://{config.KeyVaultName}.vault.azure.net/");
        var credential = new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret);

        return builder.AddAzureKeyVault(keyVaultUri, credential);
    }

    public static AzureKeyVaultConfiguration? GetAzureKeyVaultConfiguration(this IConfiguration configuration, string configSectionName = AzureKeyVaultConfiguration.DefaultConfigSectionName)
    {
        return configuration.GetSection(configSectionName)?.Get<AzureKeyVaultConfiguration>();
    }

    private static IServiceCollection AddFileSender(this IServiceCollection services,
        IConfiguration configuration)
    {
        var fileSenderType = configuration.GetSection("FileSenderOptions").Get<FileSenderOptions>();
        switch(fileSenderType?.Type)
        {
            case "mesh":
                var destinationMailbox = configuration["MESH_MAILBOX_DESTINATION"];
                var meshWorkflow = configuration["MESH_WORKFLOW"];
                var azureKeyVaultConfig = configuration.GetSection("KeyVault")?.Get<AzureKeyVaultConfiguration>();
                
                services
                    .AddTransient<IFileSender, MeshFileSender>()
                    .Configure<MeshSendOptions>(opts =>
                    {
                        opts.DestinationMailboxId = destinationMailbox;
                        opts.WorkflowId = meshWorkflow;
                    })
                    .AddMesh(configuration);
                
                if (!string.IsNullOrEmpty(azureKeyVaultConfig?.KeyVaultName))
                {
                    Console.WriteLine("Adding key vault and cert provider");
                    services.Configure<AzureKeyVaultConfiguration>(opts =>
                    {
                        opts.KeyVaultName = azureKeyVaultConfig.KeyVaultName;
                        opts.ClientId = azureKeyVaultConfig.ClientId;
                        opts.TenantId = azureKeyVaultConfig.TenantId;
                        opts.ClientSecret = azureKeyVaultConfig.ClientSecret;
                    });
                    services.AddSingleton<ICertificateProvider, KeyVaultCertificateProvider>();
                }
                else
                {
                    Console.WriteLine("Key vault configuration not set up");
                }

                break;
            case "local":
                services.AddTransient<IFileSender, LocalFileSender>();
                break;
            case "blob":
                services
                    .AddTransient<IFileSender, BlobFileSender>()
                    .Configure<BlobFileOptions>(opts =>
                        opts.ContainerName = configuration["BlobStorageConnectionString"])
                    .AddAzureClients(x =>
                    {
                        x.AddBlobServiceClient(configuration["BlobStorageConnectionString"]);
                    });
                break;
            default:
                throw new ArgumentException($"Unknown sender type: {fileSenderType}");
        }

        return services;
    }

    public static IServiceCollection AddExtractWorker<TExtractor>(this IServiceCollection services) where TExtractor : class, IExtractor 
    {
        services.AddSingleton<TExtractor>();
        services.AddHostedService<DataExtractWorker<TExtractor>>();

        return services;
    }
}
