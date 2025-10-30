using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Factories;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.BulkImport;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Json;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Okta;
using Nhs.Appointments.Core.Reports;
using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Api;

public static class FunctionConfigurationExtensions
{
    public static IFunctionsWorkerApplicationBuilder ConfigureFunctionDependencies(
        this IFunctionsWorkerApplicationBuilder builder)
    {
        // Set up configuration
        var configurationBuilder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        var configuration = configurationBuilder.Build();

        builder.Services.AddRequestInspectors();
        builder.Services.AddSingleton<IFeatureToggleHelper, FeatureToggleHelper>();
        builder.Services.AddCustomAuthentication(configuration);
        builder.Services.AddOktaUserDirectory(configuration);

        builder.Services
            .AddSingleton<IOpenApiConfigurationOptions>(_ =>
            {
                var options = new OpenApiConfigurationOptions
                {
                    Info = new OpenApiInfo { Version = "0.0.1", Title = "Manage Your Appointment API" },
                    Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                    OpenApiVersion = OpenApiVersionType.V2,
                    IncludeRequestingHostName = true,
                    ForceHttps = false,
                    ForceHttp = false,
                    DocumentFilters = new List<IDocumentFilter>
                    {
                        new TypeFixDocumentFilter(GetTypesToFixInOpenApi(), TimeProvider.System)
                    }
                };
                return options;
            });

        builder.Services
            .Configure<CosmosDataStoreOptions>(opts => opts.DatabaseName = "appts")
            .Configure<ReferenceGroupOptions>(opts => opts.InitialGroupCount = 100)
            .Configure<SiteSummaryOptions>(opts =>
            {
                opts.DaysForward = configuration.GetValue<int>("SITE_SUMMARY_DAYS_FORWARD");
                opts.DaysChunkSize = configuration.GetValue<int>("SITE_SUMMARY_DAYS_CHUNK_SIZE");
                opts.FirstRunDate = configuration.GetValue<DateOnly>("SITE_SUMMARY_FIRST_RUN_DATE");
            })
            .Configure<SiteServiceOptions>(opts =>
            {
                opts.SiteCacheKey = configuration.GetValue<string>("SITE_CACHE_KEY");
                opts.SiteCacheDuration = configuration.GetValue<int>("SITE_CACHE_DURATION_MINUTES");
                opts.DisableSiteCache = configuration.GetValue<bool>("DISABLE_SITE_CACHE");
            })
            .AddTransient<IAvailabilityStore, AvailabilityDocumentStore>()
            .AddTransient<IAvailabilityCreatedEventStore, AvailabilityCreatedEventDocumentStore>()
            .AddTransient<IBookingsDocumentStore, BookingCosmosDocumentStore>()
            .AddTransient<IReferenceNumberDocumentStore, ReferenceGroupCosmosDocumentStore>()
            .AddTransient<IEulaStore, EulaStore>()
            .AddTransient<IUserStore, UserStore>()
            .AddTransient<IRolesStore, RolesStore>()
            .AddTransient<IRolesService, RolesService>()
            .AddTransient<ISiteStore, SiteStore>()
            .AddTransient<IEmailWhitelistStore, EmailWhitelistStore>()
            .AddTransient<INotificationConfigurationStore, NotificationConfigurationStore>()
            .AddTransient<IAccessibilityDefinitionsStore, AccessibilityDefinitionsStore>()
            .AddTransient<IWellKnownOdsCodesStore, WellKnownOdsCodesStore>()
            .AddTransient<IClinicalServiceStore, ClinicalServiceStore>()
            .AddTransient<IWellKnowOdsCodesService, WellKnownOdsCodesService>()
            .AddTransient<IAggregationStore, AggregationStore>()
            .AddCosmosDataStores()
            .AddTransient<IBookingWriteService, BookingWriteService>()
            .AddTransient<IBookingQueryService, BookingQueryService>()
            .AddTransient<ISiteService, SiteService>()
            .AddTransient<IAccessibilityDefinitionsService, AccessibilityDefinitionsService>()
            .AddTransient<IAvailabilityWriteService, AvailabilityWriteService>()
            .AddTransient<IAvailabilityQueryService, AvailabilityQueryService>()
            .AddTransient<IBookingAvailabilityStateService, BookingAvailabilityStateService>()
            .AddTransient<IEulaService, EulaService>()
            .AddTransient<IAvailabilityGrouperFactory, AvailabilityGrouperFactory>()
            .AddTransient<IReferenceNumberProvider, ReferenceNumberProvider>()
            .AddTransient<IUserService, UserService>()
            .AddTransient<IPermissionChecker, PermissionChecker>()
            .AddTransient<INotificationConfigurationService, NotificationConfigurationService>()
            .AddTransient<ISiteReportService, SiteReportService>()
            .AddTransient<IBookingEventFactory, EventFactory>()
            .AddTransient<IUserDataImportHandler, UserDataImportHandler>()
            .AddTransient<ISiteDataImportHandler, SiteDataImporterHandler>()
            .AddTransient<IApiUserDataImportHandler, ApiUserDataImportHandler>()
            .AddTransient<IDataImportHandlerFactory, DataImportHandlerFactory>()
            .AddSingleton<IHasConsecutiveCapacityFilter, HasConsecutiveCapacityFilter>()
            .AddTransient<IDailySiteSummaryStore, DailySiteSummaryStore>()
            .AddTransient<ISitesSummaryTrigger, SiteSummaryTrigger>()
            .AddTransient<ISiteSummaryAggregator, SiteSummaryAggregator>()
            .AddSingleton(TimeProvider.System)
            .AddTransient<IClinicalServiceProvider, ClinicalServiceProvider>()
            .AddScoped<IMetricsRecorder, InMemoryMetricsRecorder>()
            .AddUserNotifications(configuration)
            .AddAutoMapper(typeof(CosmosAutoMapperProfile))
            .AddTransient<IAdminUserDataImportHandler, AdminUserDataImportHandler>()
            .AddTransient<ISiteStatusDataImportHandler, SiteStatusDataImportHandler>();

        var leaseManagerConnection = Environment.GetEnvironmentVariable("LEASE_MANAGER_CONNECTION");
        if (leaseManagerConnection == "local")
        {
            builder.Services.AddInMemoryLeasing();
        }
        else
        {
            builder.Services.AddAzureBlobStoreLeasing(leaseManagerConnection, "leases");
        }

        builder.Services.AddHttpClient();

        var cosmosEndpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT", EnvironmentVariableTarget.Process);
        var cosmosToken = Environment.GetEnvironmentVariable("COSMOS_TOKEN", EnvironmentVariableTarget.Process);
        var ignoreSslCertSetting =
            Environment.GetEnvironmentVariable("COSMOS_IGNORE_SSL_CERT", EnvironmentVariableTarget.Process);
        bool.TryParse(ignoreSslCertSetting, out var ignoreSslCert);
        var cosmosOptions = GetCosmosOptions(cosmosEndpoint, ignoreSslCert);

        var cosmosClient = new CosmosClient(
            accountEndpoint: cosmosEndpoint,
            authKeyOrResourceToken: cosmosToken,
            clientOptions: cosmosOptions);

        builder.Services.AddSingleton(cosmosClient);
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        SetupCosmosDatabase(cosmosClient).GetAwaiter().GetResult();

        return builder;
    }

    private static async Task SetupCosmosDatabase(CosmosClient cosmosClient)
    {
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(id: "appts");
        await database.Database.CreateContainerIfNotExistsAsync(id: "booking_data", partitionKeyPath: "/site");
        await database.Database.CreateContainerIfNotExistsAsync(id: "core_data", partitionKeyPath: "/docType");
        await database.Database.CreateContainerIfNotExistsAsync(id: "index_data", partitionKeyPath: "/docType");
        await database.Database.CreateContainerIfNotExistsAsync(id: "audit_data", partitionKeyPath: "/user");
        await database.Database.CreateContainerIfNotExistsAsync(id: "aggregated_data", partitionKeyPath: "/date");
    }

    private static CosmosClientOptions GetCosmosOptions(string cosmosEndpoint, bool ignoreSslCert)
    {
        if (ignoreSslCert)
        {
            return new CosmosClientOptions
            {
                HttpClientFactory = () => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }),
                Serializer = new CosmosJsonSerializer(),
                ConnectionMode = ConnectionMode.Gateway,
                LimitToEndpoint = true
            };
        }

        return new() { Serializer = new CosmosJsonSerializer() };
    }

    private static IEnumerable<Type> GetTypesToFixInOpenApi()
    {
        var functionEntryPoints = typeof(BaseApiFunction<,>).Assembly.GetTypes()
            .Where(t => t.GetMethods().Any(t => t.Name == "RunAsync")).Select(t => t.GetMethod("RunAsync"));
        var openApiPayloadMarkers = functionEntryPoints
            .SelectMany(t => t.GetCustomAttributes<OpenApiRequestBodyAttribute>()).Cast<OpenApiPayloadAttribute>()
            .Concat(functionEntryPoints.SelectMany(t => t.GetCustomAttributes<OpenApiResponseWithBodyAttribute>()));

        var payloadTypes = openApiPayloadMarkers
            .Select(t => t.BodyType)
            .Where(t => t.FullName.StartsWith("System") == false)
            .Distinct()
            .ToList();

        // Manually adding additional types at the moment, but it should be possible to walk through the types to derive this information
        payloadTypes.AddRange(
        [
            typeof(QueryAvailabilityResponseInfo),
            typeof(QueryAvailabilityResponseItem),
            typeof(QueryAvailabilityResponseBlock),
            typeof(Session),
            typeof(Template),
            typeof(AvailabilityCreatedEvent),
            typeof(ContactItem),
            typeof(AttendeeDetails),
        ]);

        return payloadTypes;
    }
}
