using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;
using SiteReportExtract;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var cosmosEndpoint = builder.Configuration["COSMOS_ENDPOINT"];
var cosmosToken = builder.Configuration["COSMOS_TOKEN"];

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

builder.Services
    .AddSingleton(cosmos)
    .Configure<CosmosDataStoreOptions>(opts => opts.DatabaseName = "appts")
    .AddSingleton(TimeProvider.System)
    .AddTransient<IBookingAvailabilityStateService, BookingAvailabilityStateService>()
    .AddTransient<IAvailabilityQueryService, AvailabilityQueryService>()
    .AddTransient<IAvailabilityStore, AvailabilityDocumentStore>()
    .AddTransient<IAvailabilityCreatedEventStore, AvailabilityCreatedEventDocumentStore>()
    .AddTransient<ITypedDocumentCosmosStore<DailyAvailabilityDocument>, TypedDocumentCosmosStore<DailyAvailabilityDocument>>()
    .AddTransient<IMetricsRecorder, InMemoryMetricsRecorder>()
    .AddTransient<ITypedDocumentCosmosStore<AvailabilityCreatedEventDocument>, TypedDocumentCosmosStore<AvailabilityCreatedEventDocument>>()
    .AddTransient<IBookingQueryService, BookingQueryService>()
    .AddTransient<IBookingsDocumentStore, BookingCosmosDocumentStore>()
    .AddTransient<ITypedDocumentCosmosStore<BookingDocument>, TypedDocumentCosmosStore<BookingDocument>>()
    .AddTransient<ITypedDocumentCosmosStore<BookingIndexDocument>, TypedDocumentCosmosStore<BookingIndexDocument>>()
    .AddTransient<ISiteReportService, SiteReportService>()
    .AddTransient<IClinicalServiceStore, ClinicalServiceStore>()
    .AddTransient<ITypedDocumentCosmosStore<ClinicalServiceDocument>, TypedDocumentCosmosStore<ClinicalServiceDocument>>()
    .AddTransient<ISiteService, SiteService>()
    .AddTransient<ISiteStore, SiteStore>()
    .AddTransient<ITypedDocumentCosmosStore<SiteDocument>, TypedDocumentCosmosStore<SiteDocument>>()
    .AddHostedService<Worker>()
    .AddMemoryCache()
    .AddAutoMapper(typeof(CosmosAutoMapperProfile))
    .AddAzureClients(x =>
    {
        x.AddBlobServiceClient(builder.Configuration["BlobStorageConnectionString"]);
    });

var host = builder.Build();
host.Run();

    
