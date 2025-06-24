using BookingGenerator;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

CosmosClientOptions cosmosOptions = new()
{
    HttpClientFactory = () => new HttpClient(new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    }),
    Serializer = new CosmosJsonSerializer(),
    ConnectionMode = ConnectionMode.Gateway,
    LimitToEndpoint = true,
    AllowBulkExecution = true,
};

var cosmosClient = new CosmosClient(
    accountEndpoint: builder.Configuration.GetValue<string>("COSMOS_ENDPOINT"),
    authKeyOrResourceToken: builder.Configuration.GetValue<string>("COSMOS_TOKEN"),
    clientOptions: cosmosOptions);

builder.Services
    .AddTransient<ISiteStore, SiteStore>()
    .AddTransient<IAvailabilityStore, AvailabilityDocumentStore>()
    .AddTransient<IAvailabilityCreatedEventStore, AvailabilityCreatedEventDocumentStore>()
    .AddTransient<IBookingsDocumentStore, BookingCosmosDocumentStore>()
    .AddTransient<IClinicalServiceStore, ClinicalServiceStore>()
    .AddTransient<IReferenceNumberDocumentStore, ReferenceGroupCosmosDocumentStore>()
    .AddTransient<IReferenceNumberWriteStore, ReferenceGroupCosmosDocumentStore>()
    .AddTransient<ITypedDocumentCosmosStore<AvailabilityCreatedEventDocument>, TypeFileStore<AvailabilityCreatedEventDocument>>()
    .AddTransient<ITypedDocumentCosmosStore<DailyAvailabilityDocument>, TypeFileStore<DailyAvailabilityDocument>>()
    .AddTransient<ITypedDocumentCosmosStore<BookingDocument>, TypeFileStore<BookingDocument>>()
    .AddTransient<ITypedDocumentCosmosStore<BookingIndexDocument>, TypeFileStore<BookingIndexDocument>>()
    .AddTransient<ITypedDocumentCosmosStore<ReferenceGroupDocument>, TypeFileStore<ReferenceGroupDocument>>()
    .AddTransient<ITypedDocumentCosmosStore<ClinicalServiceDocument>, TypedDocumentCosmosStore<ClinicalServiceDocument>>()
    .AddTransient<ITypedDocumentCosmosStore<SiteDocument>, TypedDocumentCosmosStore<SiteDocument>>()
    .AddMemoryCache()
    .AddTransient<ISiteService, SiteService>()
    .AddTransient<IBookingWriteService, BookingWriteService>()
    .AddTransient<IBookingQueryService, BookingQueryService>()
    .AddTransient<IReferenceNumberProvider, ReferenceNumberProvider>()
    .AddTransient<IAvailabilityWriteService, AvailabilityWriteService>()
    .AddTransient<IAvailabilityQueryService, AvailabilityQueryService>()
    .AddTransient<IBookingAvailabilityStateService, BookingAvailabilityStateService>()
    .AddTransient<IAvailabilityCalculator, AvailabilityCalculator>()
    .AddTransient<IBookingEventFactory, EventFactory>()
    .AddTransient<IMessageBus, NullMessageBus>()
    .AddInMemoryLeasing()
    .AddSingleton<IFeatureToggleHelper, FeatureToggleHelper>()
    .AddSingleton(TimeProvider.System)
    .Configure<CosmosDataStoreOptions>(opts => opts.DatabaseName = "appts")
    .Configure<ReferenceGroupOptions>(opts => opts.InitialGroupCount = 100)
    .AddSingleton(cosmosClient)
    .AddAutoMapper(typeof(CosmosAutoMapperProfile))
    .AddTransient<IMetricsRecorder, InMemoryMetricsRecorder>()
    .AddSingleton<IPartitionKeyResolver<AvailabilityCreatedEventDocument>, AvailabilityCreateEventPartitionKeyResolver>()
    .AddSingleton<IPartitionKeyResolver<BookingIndexDocument>, BookingIndexPartitionKeyResolver>()
    .AddSingleton<IPartitionKeyResolver<BookingDocument>, BookingPartitionKeyResolver>()
    .AddSingleton<IPartitionKeyResolver<DailyAvailabilityDocument>, DailyAvailabilityPartitionKeyResolver>()
    .AddSingleton<IPartitionKeyResolver<ReferenceGroupDocument>, ReferenceGroupPartitionKeyResolver>()
    .AddHostedService<Worker>();

var host = builder.Build();
host.Run();
