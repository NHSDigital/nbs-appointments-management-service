using BookingsDataExtracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;
using Parquet.Serialization;
using Refit;

namespace BookingDataExtractTests;

public class UnitTest1
{
    [Fact]
    public async Task BookingDataExtract_IntegrationTest()
    {
        var mockLifetime = new Mock<IHostApplicationLifetime>();
        var mockSendingOptions = new Mock<IOptions<MeshSendOptions>>();
        var mockMeshAuthOptions = new Mock<IOptions<MeshAuthorizationOptions>>();
        var mockMeshFactory = new Mock<IOptions<IMeshFactory>>();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "COSMOS_ENDPOINT", "https://localhost:8081" },
            { "COSMOS_TOKEN", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" },
            { "MeshClientOptions:BaseUrl", "http://localhost:4030/" },
            { "MeshAuthorizationOptions:MailboxId", "X26ABC1" },
            { "MeshAuthorizationOptions:MailboxPassword", "password" },
            { "MeshAuthorizationOptions:SharedKey", "TestKey" },
            { "MESH_MAILBOX_DESTINATION", "X26ABC2" },
            { "MESH_WORKFLOW", "MYA_Data_Bookings"}
        });

        var serviceCollection = new ServiceCollection();
        BookingsDataExtracts.ServiceRegistration.AddDataExtractServices(serviceCollection, configurationBuilder);
        
        serviceCollection.AddSingleton<IHostApplicationLifetime>(mockLifetime.Object);
        serviceCollection.AddSingleton<TestableDataExtractWorker>();

        var targetMailbox = SetupMailboxClient("X26ABC2");
        await targetMailbox.ClearMessages();

        var services = serviceCollection.BuildServiceProvider();
        var sut = services.GetRequiredService<TestableDataExtractWorker>();
        await sut.Test();
                        
        var response = await targetMailbox.CheckInboxAsync();
        
        foreach(var message in response.Messages)
        {
            var outputFile = new FileInfo(message);
            await targetMailbox.GetMessageAsFileAsync(message, outputFile);

            var results = await ParquetSerializer.DeserializeAsync<BookingExtactDataRow>(outputFile.FullName);
        }        
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
