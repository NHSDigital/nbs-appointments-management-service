using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Nbs.MeshClient.Auth;
using Nbs.MeshClient;
using Nhs.Appointments.Api.Json;
using Refit;
using Xunit.Gherkin.Quick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Parquet.Serialization;
using FluentAssertions;
using BookingsDataExtracts.Documents;

namespace BookingDataExtractTests.Specification;

[FeatureFile("./Specification/BookingExtracts.feature")]
public sealed class BookingExtractsFeatureSteps : Feature
{
    private readonly CosmosClient _cosmosClient;
    private readonly ConfigurationBuilder _configurationBuilder = new();
    private readonly List<Nhs.Appointments.Persistance.Models.BookingDocument> _testAppointments = new();
    private MeshMailbox _targetMailbox;

    public BookingExtractsFeatureSteps()
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
            Id = "BookingExtractDataTests",
            Name = "Test Site",
            Region = "North",
            IntegratedCareBoard = "ICB01",
            DocumentType = "site",
            Location = new Location("point", new[] { 21.41416002128359, -157.77021027939483 })
        };
        await _cosmosClient.GetContainer("appts", "core_data").UpsertItemAsync(site);
        return site;
    }

    [Given("I have some bookings")]
    public async Task SetupBookings(Gherkin.Ast.DataTable bookingData)
    {
        var site = await SetupSite();

        await _cosmosClient.GetContainer("appts", "booking_data").DeleteAllItemsByPartitionKeyStreamAsync(new PartitionKey(site.Id));

        var bookings = bookingData.Rows.Skip(1).Select(row =>
            new Nhs.Appointments.Persistance.Models.BookingDocument
            {
                Id = row.Cells.ElementAt(1).Value,
                Reference = row.Cells.ElementAt(1).Value,
                Created = DateTime.UtcNow.AddHours(int.Parse(row.Cells.ElementAt(0).Value)*-1),
                From = DateTime.Today.AddDays(5).AddHours(9),
                Duration = 5,
                Service = "COVID:18_74",
                ReminderSent = true,
                Site = site.Id,
                DocumentType = "booking",
                Status = Enum.Parse<Nhs.Appointments.Core.AppointmentStatus>(row.Cells.ElementAt(2).Value),
                StatusUpdated = DateTime.MinValue,
                ContactDetails = [],
                AttendeeDetails = new Nhs.Appointments.Core.AttendeeDetails
                {
                    FirstName = "Test",
                    LastName = "User",
                    NhsNumber = "1234567890",
                    DateOfBirth = new DateOnly(1977, 01, 24)
                },
                AdditionalData = new NbsAdditionalData
                {
                    IsAppBooking = true,
                    ReferralType = "",
                    IsCallCentreBooking = false
                }            
            });

        foreach (var booking in bookings)
        {
            await _cosmosClient.GetContainer("appts", "booking_data").CreateItemAsync(booking);
            _testAppointments.Add(booking);
        }
    }

    [And("the system is configured as follows")]
    public void ConfigureMesh(Gherkin.Ast.DataTable table)
    {
        var targetMailboxId = table.Rows.ElementAt(1).Cells.ElementAt(0).Value;
        var workflowId = table.Rows.ElementAt(1).Cells.ElementAt(1).Value;
        _configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
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

    [When("the booking data extract runs")]
    public async Task RunDataExtract()
    {
        var serviceCollection = new ServiceCollection();
        BookingsDataExtracts.ServiceRegistration.AddDataExtractServices(serviceCollection, _configurationBuilder);

        var mockLifetime = new Mock<IHostApplicationLifetime>();
        serviceCollection.AddSingleton(mockLifetime.Object);
        serviceCollection.AddSingleton<TestableDataExtractWorker>();        

        var services = serviceCollection.BuildServiceProvider();
        var sut = services.GetRequiredService<TestableDataExtractWorker>();
        await sut.Test();
    }

    [Then("booking data is available in the target mailbox")]
    public async Task AssertData(Gherkin.Ast.DataTable expectedAppointments)
    {
        var response = await _targetMailbox.CheckInboxAsync();
        response.Messages.Count.Should().Be(1);        

        var outputFolder = new DirectoryInfo("./recieved");
        outputFolder.Create();
        var file = await _targetMailbox.GetMessageAsFileAsync(response.Messages.First(), outputFolder);

        var pfile = new FileInfo(Path.Combine(outputFolder.FullName, file));
        var results = await ParquetSerializer.DeserializeAsync<BookingExtactDataRow>(pfile.FullName);
        results.Count.Should().Be(expectedAppointments.Rows.Count() - 1);

        results.Select(r => r.BOOKING_REFERENCE_NUMBER).Should().BeEquivalentTo(expectedAppointments.Rows.Skip(1).Select(r => r.Cells.ElementAt(0).Value));                       
    }

    [Then("an empty file is recieved")]
    public async Task AssertEmptyFile()
    {
        var response = await _targetMailbox.CheckInboxAsync();
        response.Messages.Count.Should().Be(1);

        // TODO: Check that the file is empty
    }

    [Then("a file is recieved with the name 'MYA_booking_{yyyy-MM-ddTHHmm}.parquet'")]
    public async Task AssertFileName()
    {
        var response = await _targetMailbox.CheckInboxAsync();
        response.Messages.Count.Should().Be(1);

        // TODO: Check that the file name
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
