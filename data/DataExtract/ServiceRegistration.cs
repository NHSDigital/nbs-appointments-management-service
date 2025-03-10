using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Nhs.Appointments.Api.Json;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataExtract;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataExtractServices(this IServiceCollection services, string fileName, IConfigurationBuilder configurationBuilder)
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
            .Configure<FileNameOptions>(opts => 
            { 
                opts.FileName = fileName;
            })
            .AddSingleton(TimeProvider.System)
            .AddSingleton(cosmos)
            .AddMesh(configuration);

        var azureKeyVaultConfig = configuration.GetSection("KeyVault")?.Get<AzureKeyVaultConfiguration>();
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
            Console.WriteLine("Key vault configuration not set up");

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

    public static IServiceCollection AddExtractWorker<TExtractor>(this IServiceCollection services) where TExtractor : class, IExtractor 
    {
        services.AddSingleton<TExtractor>();
        services.AddHostedService<DataExtractWorker<TExtractor>>();

        return services;
    }
}
