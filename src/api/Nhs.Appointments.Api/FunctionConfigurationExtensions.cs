using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;
using Microsoft.Azure.Cosmos;
using System.Net.Http;
using Nhs.Appointments.Api.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Api.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging;
using System.Collections.Generic;
using Nhs.Appointments.Api.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Nhs.Appointments.Api.Functions;

namespace Nhs.Appointments.Api;

public static class FunctionConfigurationExtensions
{
    public static IFunctionsWorkerApplicationBuilder ConfigureFunctionDependencies(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.Services.AddCustomAuthentication();            
        
        builder.Services
            .AddSingleton<IOpenApiConfigurationOptions>(_ => 
            {
                var options = new OpenApiConfigurationOptions()
                {
                    Info = new OpenApiInfo()
                    {
                        Version = "0.0.1",
                        Title = "Manage Your Appointment API"
                    },
                    Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                    OpenApiVersion = OpenApiVersionType.V2,
                    IncludeRequestingHostName = true,
                    ForceHttps = false,
                    ForceHttp = false,
                    DocumentFilters = new List<IDocumentFilter> { new TypeFixDocumentFilter(GetTypesToFixInOpenApi(), TimeProvider.System) }
                };
                return options;
            });

        builder.Services
            .Configure<CosmosDataStoreOptions>(opts => opts.DatabaseName = "appts")
            .Configure<ReferenceGroupOptions>(opts => opts.InitialGroupCount = 100)
            .AddTransient<IAvailabilityStore, AvailabilityDocumentStore>()
            .AddTransient<IAvailabilityCreatedEventStore, AvailabilityCreatedEventDocumentStore>()
            .AddTransient<IBookingsDocumentStore, BookingCosmosDocumentStore>()
            .AddTransient<IReferenceNumberDocumentStore, ReferenceGroupCosmosDocumentStore>()
            .AddTransient<IEulaStore, EulaStore>()
            .AddTransient<IUserStore, UserStore>()
            .AddTransient<IRolesStore, RolesStore>()
            .AddTransient<IRolesService, RolesService>()
            .AddTransient<ISiteStore, SiteStore>()
            .AddTransient<INotificationConfigurationStore, NotificationConfigurationStore>()
            .AddTransient<IAttributeDefinitionsStore, AttributeDefinitionsStore>()
            .AddCosmosDataStores()                        
            .AddTransient<IBookingsService, BookingsService>()
            .AddTransient<ISiteService, SiteService>()
            .AddTransient<IAttributeDefinitionsService, AttributeDefinitionsService>()
            .AddTransient<IAvailabilityService, AvailabilityService>()
            .AddTransient<IEulaService, EulaService>()
            .AddTransient<IAvailabilityCalculator, AvailabilityCalculator>()
            .AddTransient<IAvailabilityGrouperFactory, AvailabilityGrouperFactory>()
            .AddTransient<IReferenceNumberProvider, ReferenceNumberProvider>()
            .AddTransient<IUserService, UserService>()
            .AddTransient<IPermissionChecker, PermissionChecker>()     
            .AddTransient<IBookingEventFactory, EventFactory>()
            .AddSingleton(TimeProvider.System)
            .AddScoped<IMetricsRecorder, InMemoryMetricsRecorder>()
            .AddUserNotifications()
            .AddAutoMapper(typeof(CosmosAutoMapperProfile));

        var leaseManagerConnection = Environment.GetEnvironmentVariable("LEASE_MANAGER_CONNECTION");
        if (leaseManagerConnection == "local")
            builder.Services.AddInMemoryLeasing();
        else
            builder.Services.AddAzureBlobStoreLeasing(leaseManagerConnection, "leases");

        builder.Services.AddHttpClient();

        var cosmosEndpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT", EnvironmentVariableTarget.Process);
        var cosmosToken = Environment.GetEnvironmentVariable("COSMOS_TOKEN", EnvironmentVariableTarget.Process);
        var ignoreSslCertSetting = Environment.GetEnvironmentVariable("COSMOS_IGNORE_SSL_CERT", EnvironmentVariableTarget.Process);
        
        var cosmosClient = new CosmosClient(
                accountEndpoint: cosmosEndpoint,
                authKeyOrResourceToken: cosmosToken,
                clientOptions: new CosmosClientOptions
                {
                    Serializer = new CosmosJsonSerializer()
                });

        builder.Services.AddSingleton(cosmosClient);
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        SetupCosmosDatabase(cosmosClient).GetAwaiter().GetResult();    

    return builder;
    }

    private static async Task SetupCosmosDatabase(CosmosClient cosmosClient)
    {
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(id: "appts");
        await database.Database.CreateContainerIfNotExistsAsync(id: "booking_data", partitionKeyPath: "/site");
        await database.Database.CreateContainerIfNotExistsAsync(id: "index_data", partitionKeyPath: "/docType");
    }

    private static IEnumerable<Type> GetTypesToFixInOpenApi()
    {
        var functionEntryPoints = typeof(BaseApiFunction<,>).Assembly.GetTypes().Where(t => t.GetMethods().Any(t => t.Name == "RunAsync")).Select(t => t.GetMethod("RunAsync"));
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
