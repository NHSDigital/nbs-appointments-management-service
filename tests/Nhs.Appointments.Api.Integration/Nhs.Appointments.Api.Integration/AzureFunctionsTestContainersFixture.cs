using System.Net.Http.Headers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.ApiClient;
using Nhs.Appointments.ApiClient.Auth;
using Nhs.Appointments.ApiClient.Impl;
using Testcontainers.CosmosDb;

namespace Nhs.Appointments.Api.Integration
{
    public class AzureFunctionsTestcontainersFixture : IAsyncLifetime
    {
        private readonly IFutureDockerImage _azureFunctionsDockerImage;

        public INhsAppointmentsApi ApiClient { get; private set; }
        private const string SigningKey = "2EitbEouxHQ0WerOy3TwcYxh3/wZA0LaGrU1xpKg0KJ352H/mK0fbPtXod0T0UCrgRHyVjF6JfQm/LillEZyEA==";
        private const string ClientId = "test-client";
        private Uri _baseAddress;
        private Uri _cosmosAddress;

        public IContainer AzureFunctionsContainerInstance { get; private set; }
        public CosmosDbContainer CosmosDbEmulatorContainer { get; private set; }
        
        public AzureFunctionsTestcontainersFixture()
        {
            // build a Docker image of the Appointments API:

            _azureFunctionsDockerImage = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), String.Empty)
                .WithDockerfile("AzureFunctions-TestContainers.Dockerfile")
                .WithBuildArgument(
                    "RESOURCE_REAPER_SESSION_ID",
                    ResourceReaper.DefaultSessionId.ToString("D"))
                .Build();
        }

        public async Task InitializeAsync()
        {
            // create the API Docker image:
            await _azureFunctionsDockerImage.CreateAsync();
            
            // build a CosmosDB emulator container:
            CosmosDbEmulatorContainer =  new CosmosDbBuilder()
                // x86 only:
                .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
                .WithExposedPort(443)
                .WithPortBinding(443, true)
                // ARM / Apple:
                //.WithImage("ghcr.io/pikami/cosmium")
                
                .Build();
            await CosmosDbEmulatorContainer.StartAsync();
            
            // grab the CosmosDB endpoint:
            _cosmosAddress = new UriBuilder(
                Uri.UriSchemeHttps,
                CosmosDbEmulatorContainer.Hostname,
                CosmosDbEmulatorContainer.GetMappedPublicPort(443)
            ).Uri;
            
            // spin up a containerised instance of the Appointments API,
            // configured to use the CosmosDb emulator 
            AzureFunctionsContainerInstance = new ContainerBuilder()
                .WithEnvironment("COSMOS_ENDPOINT", _cosmosAddress.ToString())
                .WithImage(_azureFunctionsDockerImage)
                .WithPortBinding(80, true)
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilHttpRequestIsSucceeded(r => r.ForPort(80)))
                .Build();

            await AzureFunctionsContainerInstance.StartAsync();
            
            // grab the endpoint of our API:
            _baseAddress = new UriBuilder(
                Uri.UriSchemeHttp,
                AzureFunctionsContainerInstance.Hostname,
                AzureFunctionsContainerInstance.GetMappedPublicPort(80)
            ).Uri;

            // create an API client for our API instance:            
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());

            ILogger<BookingsApiClient> bookingsLogger = loggerFactory.CreateLogger<BookingsApiClient>();
            ILogger<SitesApiClient> sitesLogger = loggerFactory.CreateLogger<SitesApiClient>();
            ILogger<TemplatesApiClient> templatesLogger = loggerFactory.CreateLogger<TemplatesApiClient>();

            ApiClient = new NhsAppointmentsApi(new BookingsApiClient(CreateHttpClient, bookingsLogger), new SitesApiClient(CreateHttpClient, sitesLogger), new TemplatesApiClient(CreateHttpClient, templatesLogger));
        }

        public async Task DisposeAsync()
        {
            await AzureFunctionsContainerInstance.DisposeAsync();
            await _azureFunctionsDockerImage.DisposeAsync();
            await CosmosDbEmulatorContainer.DisposeAsync();
        }

        private HttpClient CreateHttpClient()
        {
            var requestSigningHandler = new RequestSigningHandler(new RequestSigner(TimeProvider.System, SigningKey));
            requestSigningHandler.InnerHandler = new HttpClientHandler();
            var client = new HttpClient(requestSigningHandler);
            client.BaseAddress = _baseAddress;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("ClientId", new[] { ClientId });
            return client;
        }
    }
}
