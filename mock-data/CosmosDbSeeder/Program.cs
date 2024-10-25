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
            .AddEnvironmentVariables()
            .Build();

        var cosmosEndpoint = configuration["COSMOS_ENDPOINT"];
        var cosmosToken = configuration["COSMOS_TOKEN"];
        var databaseName = configuration["CosmosSettings:DatabaseName"];
        var containers = configuration.GetSection("CosmosSettings:Containers").Get<List<ContainerConfig>>();


        var cosmosOptions = GetCosmosOptions();

        _cosmosClient = new CosmosClient(
            accountEndpoint: cosmosEndpoint,
            authKeyOrResourceToken: cosmosToken,
            clientOptions: cosmosOptions);

        _database = await CreateDatabaseAsync(databaseName);
        /*foreach (var container in containers)
        {
            await DeleteContainers(container.Name);
            await AddItemsToContainerAsync(container.Name, container.PartitionKey);
        }*/
        await CreateContainerAsync("booking_data", "/site");
        await CreateBookings();

        Console.WriteLine("Database seeded successfully");
    }

    private static async Task CreateBookings()
    {
        var date = new DateTime(2025, 1, 1, 9, 0, 0);
        while(date.Month == 1)
        {
            while(date.Hour < 18)
            {
                if(date.Minute < 40)
                {
                    var booking = new
                    {
                        id = Guid.NewGuid().ToString(),
                        site = "ABC01",
                        reference = Guid.NewGuid().ToString(),
                        from = date,
                        duration = 5,
                        service = "COVID:12_15",
                        attendeeDetails = new
                        {
                            nhsNumber = "12213987",
                            firstName = "Mr",
                            lastName = "Test",
                            dateOfBirth = new DateOnly(1977, 1, 24)
                        }
                    };
                    await _cosmosClient.GetContainer("appts", "booking_data").UpsertItemAsync(booking);                    
                }
                date = date.AddMinutes(5);
            }
            date = date.AddHours(15);
        }
    }

    private static async Task<Database> CreateDatabaseAsync(string? databaseId)
    {
        if (_cosmosClient is null)
            throw new Exception("Cosmos Client was not initialised");

        var response = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

        Console.WriteLine(response.StatusCode == HttpStatusCode.Created
            ? $"Created database {databaseId}"
            : $"Skipped creating database {databaseId} as it already exists");

        return response.Database;
    }
    
    private static async Task DeleteContainers(string containerId)
    {
        if (_database is null)
            throw new Exception("Database was not initialised");
        
        var container = _database.GetContainer(containerId);
        
        try
        { 
            await container.DeleteContainerAsync();
            Console.WriteLine($"Deleted container {containerId}");
        }
        catch (CosmosException ex) when ( ex.StatusCode == HttpStatusCode.NotFound )
        {
            Console.WriteLine($"Skipped deleting container {containerId} as it does not exist");
        }
    }

    private static async Task<Container> CreateContainerAsync(string containerId, string partitionKeyPath)
    {
        if (_database is null)
            throw new Exception("Database was not initialised");

        var response = await _database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath);

        Console.WriteLine(response.StatusCode == HttpStatusCode.Created
            ? $"Created container {containerId} with partition key {partitionKeyPath}"
            : $"Skipped creating container {containerId} as it already exists");

        return response.Container;
    }

    private static async Task AddItemsToContainerAsync(string containerName, string partitionKeyPath)
    {
        var container = await CreateContainerAsync(containerName, partitionKeyPath);

        var folderPath = Path.Combine(AppContext.BaseDirectory, $"items/{containerName}");
        if(Directory.Exists(folderPath))
        {
            var jsonFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, $"items/{containerName}"), "*.json");

            foreach (var file in jsonFiles)
            {
                var fileName = file.Split($"{containerName}").Last();
                var item = JsonConvert.DeserializeObject<JObject>(await File.ReadAllTextAsync(file));
                var response = await container.UpsertItemAsync(item);
                Console.WriteLine(response.StatusCode == HttpStatusCode.Created
                    ? $"Created {fileName} in {containerName}"
                    : $"Replaced {fileName} in {containerName}");
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