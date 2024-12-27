using BookingsDataExtracts;
using BookingsDataExtracts.Documents;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Nhs.Appointments.Api.Json;

Console.WriteLine("Starting...");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

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

var bookingsStore = new CosmosStore<BookingDocument>(cosmos, "appts", "booking_data");
var sitesStore = new CosmosStore<SiteDocument>(cosmos, "appts", "core_data");
var availabilityStore = new CosmosStore<DailyAvailabilityDocument>(cosmos, "appts", "booking_data");

var bookingDataExtract = new BookingDataExtract(bookingsStore, sitesStore);
await bookingDataExtract.RunAsync();

Console.WriteLine("done");
