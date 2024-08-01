using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosDbSeeder;

class Program
{
    private static CosmosClient? _cosmosClient;
    private static Database? _database;

    static async Task Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var cosmosEndpoint = configuration["CosmosSettings:Endpoint"];
        var cosmosToken = configuration["CosmosSettings:Token"];
        var databaseName = configuration["CosmosSettings:DatabaseName"];
        var containers = configuration.GetSection("CosmosSettings:Containers").Get<List<ContainerConfig>>();


        var cosmosOptions = GetCosmosOptions();

        _cosmosClient = new CosmosClient(
            accountEndpoint: cosmosEndpoint,
            authKeyOrResourceToken: cosmosToken,
            clientOptions: cosmosOptions);

        await CreateDatabaseAsync(databaseName);
        foreach (var container in containers)
        {
            await AddItemsToContainerAsync(container.Name, container.PartitionKey);
        }

        Console.WriteLine("Database seeded successfully");
    }

    private static async Task CreateDatabaseAsync(string? databaseId)
    {
        if (_cosmosClient is null)
            throw new Exception("Cosmos Client was not initialised");

        _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        Console.WriteLine("Created Database: {0}\n", _database.Id);
    }

    private static async Task<Container> CreateContainerAsync(string containerId, string partitionKeyPath)
    {
        if (_database is null)
            throw new Exception("Database was not initialised");

        var container = await _database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath);
        Console.WriteLine("Created Container: {0}\n", containerId);
        return container;
    }

    private static async Task AddItemsToContainerAsync(string containerName, string partitionKeyPath)
    {
        var container = await CreateContainerAsync(containerName, partitionKeyPath);
        var jsonFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, $"items/{containerName}"), "*.json");

        foreach (var file in jsonFiles)
        {
            var fileName = file.Split($"{containerName}").Last();
            var item = JsonConvert.DeserializeObject<JObject>(await File.ReadAllTextAsync(file));
            try
            {
                await container.CreateItemAsync(item);
                Console.WriteLine($"Added {fileName} to {containerName}");
            }
            catch (CosmosException e)
                when (e.StatusCode == HttpStatusCode.Conflict)
            {
                await container.ReplaceItemAsync(item, item.GetValue("id").ToString());
                Console.WriteLine($"Replaced {fileName} in {containerName}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR writing {fileName} to {containerName} - {e}");
            }
        }
    }

    private static CosmosClientOptions GetCosmosOptions()
    {
        return new CosmosClientOptions()
        {
            HttpClientFactory = () => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }),
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true
        };
    }
}

public class ContainerConfig
{
    public required string Name { get; set; }
    public required string PartitionKey { get; set; }
}