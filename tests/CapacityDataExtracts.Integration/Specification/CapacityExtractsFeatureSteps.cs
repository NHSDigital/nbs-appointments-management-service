using CapacityDataExtracts.Documents;
using DataExtract;
using DataExtract.Documents;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Parquet.Serialization;
using Refit;
using Xunit.Gherkin.Quick;

namespace CapacityDataExtracts.Integration.Specification;

[FeatureFile("./Specification/BookingExtracts.feature")]
public sealed class CapacityExtractsFeatureSteps
{
    private readonly CosmosClient _cosmosClient;
    private readonly ConfigurationBuilder _configurationBuilder = new();
    private readonly List<DailyAvailabilityDocument> _testCapacity = new();
    private MeshMailbox _targetMailbox;

    public CapacityExtractsFeatureSteps()
    {
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

        _cosmosClient = new(
            accountEndpoint: CosmosEndpoint,
            authKeyOrResourceToken: CosmosToken,
            clientOptions: options);
    }

    private string CosmosEndpoint => Environment.GetEnvironmentVariable("COSMOS_ENDPOINT") ?? "https://localhost:8081/";
    private string CosmosToken => Environment.GetEnvironmentVariable("COSMOS_TOKEN") ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    public async Task<SiteDocument> SetupSite()
    {
        var site = new SiteDocument
        {
            Id = $"CapacityExtractDataTests",
            OdsCode = "DET01",
            Name = "Test Site",
            Region = "North",
            IntegratedCareBoard = "ICB01",
            DocumentType = "site",
            Location = new Location("point", new[] { 21.41416002128359, -157.77021027939483 })
        };
        await _cosmosClient.GetContainer("appts", "core_data").UpsertItemAsync(site);
        return site;
    }

    [Given("I have some capacity")]
    public async Task SetupCapacity(Gherkin.Ast.DataTable capacityData) 
    {
        var site = await SetupSite();
        await DeleteCapacityFromPreviousTests();

        var capacitys = capacityData.Rows.Skip(1).Select(row =>
            new DailyAvailabilityDocument
            {
                Date = DateOnly.ParseExact(row.Cells.ElementAt(0).Value, "yyyy-MM-dd", null),
                Sessions = [ 
                    new Session 
                    {
                        From = TimeOnly.ParseExact(row.Cells.ElementAt(0).Value, "hh:mm:ss", null),
                        Until = TimeOnly.ParseExact(row.Cells.ElementAt(1).Value, "hh:mm:ss", null),
                        Services = ["service1", "service2"],
                        SlotLength = (TimeOnly.ParseExact(row.Cells.ElementAt(0).Value, "hh:mm:ss", null) - TimeOnly.ParseExact(row.Cells.ElementAt(1).Value, "hh:mm:ss", null)).Minutes,
                        Capacity = 5
                    }
                ]
            });

        foreach (var capacity in capacitys)
        {
            await _cosmosClient.GetContainer("appts", "booking_data").CreateItemAsync(capacity);
            _testCapacity.Add(capacity);
        }
    }

    [And("the system is configured as follows")]
    public void ConfigureMesh(Gherkin.Ast.DataTable table)
    {
        var targetMailboxId = table.Rows.ElementAt(1).Cells.ElementAt(0).Value;
        var workflowId = table.Rows.ElementAt(1).Cells.ElementAt(1).Value;
        _configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "COSMOS_ENDPOINT", CosmosEndpoint },
            { "COSMOS_TOKEN", CosmosToken },
            { "MeshClientOptions:BaseUrl", "http://localhost:4030/" },
            { "MeshAuthorizationOptions:MailboxId", "X26ABC1" },
            { "MeshAuthorizationOptions:MailboxPassword", "password" },
            { "MeshAuthorizationOptions:SharedKey", "TestKey" },
            { "MESH_MAILBOX_DESTINATION", targetMailboxId },
            { "MESH_WORKFLOW", workflowId }
        });

        _targetMailbox = SetupMailboxClient(targetMailboxId);
    }

    [And("the target mailbox is empty")]
    public async Task ClearMailbox()
    {
        await _targetMailbox.ClearMessages();
    }

    [When(@"the capacity data extract runs on '(\d{4}-\d{2}-\d{2} \d{2}:\d{2})'")]
    public async Task RunDataExtract(string dateTime)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDataExtractServices("BookingCapacity", _configurationBuilder)
            .AddCosmosStore<DailyAvailabilityDocument>()
            .AddCosmosStore<SiteDocument>()
            .AddExtractWorker<CapacityDataExtract>();

        var mockLifetime = new Mock<IHostApplicationLifetime>();
        serviceCollection.AddSingleton(mockLifetime.Object);

        var mockTimeProvider = new Mock<TimeProvider>();
        var date = DateTimeOffset.ParseExact(dateTime, "yyyy-MM-dd HH:mm", null);
        mockTimeProvider.Setup(x => x.GetUtcNow()).Returns(date);

        serviceCollection.AddSingleton(mockTimeProvider.Object);

        serviceCollection.AddSingleton<TestableDataExtractWorker>();

        var services = serviceCollection.BuildServiceProvider();
        var sut = services.GetRequiredService<TestableDataExtractWorker>();
        await sut.Test();
    }

    [Then("capacity data is available in the target mailbox")]
    public async Task AssertData(Gherkin.Ast.DataTable expectedAppointments)
    {
        var response = await _targetMailbox.CheckInboxAsync();
        response.Messages.Count.Should().Be(1);

        var results = await ReadFromResultsFile(response.Messages.First());
        results.Count.Should().Be(expectedAppointments.Rows.Count() - 1);

        results.Select(r => r.DATE).Should().BeEquivalentTo(expectedAppointments.Rows.Skip(1).Select(r => r.Cells.ElementAt(0).Value));
        results.Select(r => r.TIME).Should().BeEquivalentTo(expectedAppointments.Rows.Skip(1).Select(r => r.Cells.ElementAt(1).Value));
    }

    [Then("an empty file is recieved")]
    public async Task AssertEmptyFile()
    {
        var response = await _targetMailbox.CheckInboxAsync();
        response.Messages.Count.Should().Be(1);

        var results = await ReadFromResultsFile(response.Messages.First());
        results.Count.Should().Be(0);
    }

    [Then(@"a file is recieved with the name '(.+)'")]
    public async Task AssertFileName(string expectedFileName)
    {
        var response = await _targetMailbox.CheckInboxAsync();
        response.Messages.Count.Should().Be(1);

        var outputFolder = new DirectoryInfo("./recieved");
        outputFolder.Create();
        var actualFileName = await _targetMailbox.GetMessageAsFileAsync(response.Messages.First(), outputFolder);
        actualFileName.Should().Be(expectedFileName);
    }

    private async Task DeleteCapacityFromPreviousTests()
    {
        var container = _cosmosClient.GetContainer("appts", "booking");
        var iterator = container.GetItemLinqQueryable<DailyAvailabilityDocument>().Where(bd => bd.DocumentType == "daily_availability").ToFeedIterator();

        var capacityToDelete = new List<string>();
        using (iterator)
        {
            while (iterator.HasMoreResults)
            {
                var resultSet = await iterator.ReadNextAsync();
                capacityToDelete.AddRange(resultSet.Select(bd => bd.Id));
            }
        }

        var partitionKey = new PartitionKey("CapacityExtractDataTests");
        foreach (var id in capacityToDelete)
            await container.DeleteItemAsync<DailyAvailabilityDocument>(id, partitionKey);
    }

    private async Task<List<SiteSessionParquet>> ReadFromResultsFile(string messageId)
    {
        var outputFolder = new DirectoryInfo("./recieved");
        outputFolder.Create();
        var file = await _targetMailbox.GetMessageAsFileAsync(messageId, outputFolder);

        var pfile = new FileInfo(Path.Combine(outputFolder.FullName, file));
        return (await ParquetSerializer.DeserializeAsync<SiteSessionParquet>(pfile.FullName)).ToList();
    }

    private MeshMailbox SetupMailboxClient(string mailboxId)
    {
        var meshTokenGenerator = new MeshAuthorizationTokenGenerator(new MeshAuthorizationOptions
        {
            MailboxId = mailboxId,
            MailboxPassword = "password",
            SharedKey = "TestKey",
            CertificateName = "None"
        });

        var meshHttpClient = RestService.CreateHttpClient("http://localhost:4030/", new()
        {
            AuthorizationHeaderValueGetter = (request, _) =>
            {
                return Task.FromResult(meshTokenGenerator.GenerateAuthorizationToken());
            }
        });

        var meshClient = RestService.For<IMeshClient>(meshHttpClient);
        return new MeshMailbox(mailboxId, Mock.Of<ILogger<MeshMailbox>>(), meshClient);
    }
}
