using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core.Json;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Jobs.BlobAuditor.Integration.Scenarios;

public abstract class BaseFeatureSteps : Feature
{
    private readonly string _databaseName = "appt";
    private readonly CosmosClient _cosmosClient;
    private readonly BlobServiceClient _blobServiceClient;
    protected readonly List<Dictionary<string, string>> _testFiles = new();

    private string CosmosEndpoint => Environment.GetEnvironmentVariable("COSMOS_ENDPOINT") ?? "https://localhost:8081/";

    private string CosmosToken => Environment.GetEnvironmentVariable("COSMOS_TOKEN") ??
                                  "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    
    private string BlobConnectionString => Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STRING") ??
                                  "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

    protected virtual string ContainerName => "";
    
    protected BaseFeatureSteps()
    {
        CosmosClientOptions options = new()
        {
            HttpClientFactory = () => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }),
            Serializer = new CosmosJsonSerializer(),
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true
        };

        _cosmosClient = new(
            accountEndpoint: CosmosEndpoint,
            authKeyOrResourceToken: CosmosToken,
            clientOptions: options);
        
        _blobServiceClient = new BlobServiceClient(BlobConnectionString);
    }

    protected async Task UploadFileToContainerAsync(Dictionary<string, string> file)
    {
        var containerClient = GetContainer();
        await containerClient.CreateItemAsync(file);
        
        _testFiles.Add(file);
    }
    
    protected BlobContainerClient GetBlobContainer()
    {
        return _blobServiceClient.GetBlobContainerClient(
            $"{DateTime.UtcNow:yyyyMMdd}-{ContainerName.Replace("_", "")}");
    }
    
    protected Container GetContainer() => _cosmosClient.GetContainer(_databaseName, ContainerName);
}
