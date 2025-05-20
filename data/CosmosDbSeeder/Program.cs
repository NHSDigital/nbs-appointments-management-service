using System.Net;
using DotMarkdown;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosDbSeeder;

class Program
{
    private static CosmosClient _cosmosClient;
    private static Database _database;
    private static Report _report;
    private static WriteMode _writeMode;

    static async Task Main(string[] args)
    {
        _writeMode = args.Contains("--no-overwrites") ? WriteMode.ErrorOnOverwrite : WriteMode.AllowOverwrite; 
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var cosmosEndpoint = configuration["COSMOS_ENDPOINT"];
        var cosmosToken = configuration["COSMOS_TOKEN"];
        var environment = configuration["ENVIRONMENT"];
        var databaseName = configuration["CosmosSettings:DatabaseName"];
        var containers = configuration.GetSection("CosmosSettings:Containers").Get<List<ContainerConfig>>();
        _report = new Report();

        var cosmosOptions = GetCosmosOptions();

        _cosmosClient = new CosmosClient(
            accountEndpoint: cosmosEndpoint,
            authKeyOrResourceToken: cosmosToken,
            clientOptions: cosmosOptions);

        _database = await CreateDatabaseAsync(databaseName);
        foreach (var container in containers)
        {
            var knownEnvironments = new [] { "local", "dev", "int", "perf", "stag", "prod" };
            
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentException("Environment must be provided");
            
            if (knownEnvironments.Contains(environment) == false)
                throw new ArgumentOutOfRangeException($"Environment {environment} is not supported");

            if (args.Contains("delete-containers"))
            {
                if(IsProtectedEnvironment(environment))
                    throw new InvalidOperationException("Cannot delete container from a protected environment");
                await DeleteContainers(container.Name);
            }

            await AddItemsToContainerAsync(container.Name, container.PartitionKey, environment);
        }
        
        OutputReport("data_import_report.md", true);
        OutputReport("data_import_summary.md", false);

        Console.WriteLine("Database seeded successfully");
    }

    private static void OutputReport(string filePath, bool includeErrors)
    {
        using var reportWriter = MarkdownWriter.Create(filePath);
        reportWriter.WriteHeading1("Data Import Report");
        reportWriter.WriteBold(_report.TotalDocumentsFound.ToString());
        reportWriter.WriteString($" items found to import");
        reportWriter.WriteLine();
        reportWriter.WriteLine();
        reportWriter.WriteBold(_report.DocumentsImported.ToString());
        reportWriter.WriteString($" items imported successfully");
        reportWriter.WriteLine();
        reportWriter.WriteLine();

        if (includeErrors && _report.DocumentsImported != _report.TotalDocumentsFound)
        {
            reportWriter.WriteHeading2("Import errors");
            
            foreach (var item in _report.DocumentErrors)
            {
                reportWriter.WriteHeading3(item.Item1);
                reportWriter.WriteBulletItem(item.Item2);
            }
            reportWriter.WriteLine();
            reportWriter.WriteLine();
        }
    }

    private static async Task<Database> CreateDatabaseAsync(string databaseId)
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

    private static async Task AddItemsToContainerAsync(string containerName, string partitionKeyPath, string environment)
    {
        var container = await CreateContainerAsync(containerName, partitionKeyPath);
        var folderPath = Path.Combine(AppContext.BaseDirectory, $"items/{environment}/{containerName}");
        if(Directory.Exists(folderPath))
        {
            var jsonFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, $"items/{environment}/{containerName}"), "*.json");

            foreach (var file in jsonFiles)
            {
                var fileName = file.Split($"{containerName}").Last();
                try
                {
                    _report.TotalDocumentsFound++;
                    var item = JsonConvert.DeserializeObject<JObject>(await File.ReadAllTextAsync(file));

                    var response = _writeMode == WriteMode.AllowOverwrite ? await container.UpsertItemAsync(item) : await container.CreateItemAsync(item);
                    Console.WriteLine(response.StatusCode == HttpStatusCode.Created ? $"Created {fileName} in {containerName}" : $"Replaced {fileName} in {containerName}");
                    _report.DocumentsImported++;
                }
                catch (Exception ex)
                {
                    _report.DocumentErrors.Add((fileName, ex.Message));
                }
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

    private static bool IsProtectedEnvironment(string environment)
    {
        var protectedEnvironments = new [] { "dev", "int", "stag", "prod" };
        return protectedEnvironments.Contains(environment);
    }
}

public enum WriteMode
{
    ErrorOnOverwrite,
    AllowOverwrite
}

public class ContainerConfig
{
    public required string Name { get; set; }
    public required string PartitionKey { get; set; }
}

public class Report
{
    public int TotalDocumentsFound { get; set; }
    public int DocumentsImported { get; set; }
    public List<(string, string)> DocumentErrors { get; set; }  = new List<(string, string)>();
}
