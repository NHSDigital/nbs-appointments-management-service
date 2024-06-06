using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;
using Microsoft.Azure.Cosmos;
using System.Net.Http;
using Nhs.Appointments.Api.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Api.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace Nhs.Appointments.Api;

public static class FunctionConfigurationExtensions
{
    public static IFunctionsWorkerApplicationBuilder ConfigureFunctionDependencies(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.Services
            .Configure<ApiKeyOptions>(opts =>
            {
                opts.ValidKeys = Environment.GetEnvironmentVariable("API_KEYS")?.Split(",") ?? new string[0];
            })
            .Configure<AuthOptions>(opts =>
            {
                opts.ProviderUri = Environment.GetEnvironmentVariable("AuthProvider_ProviderUri");
                opts.Issuer = Environment.GetEnvironmentVariable("AuthProvider_Issuer");
                opts.AuthorizePath = Environment.GetEnvironmentVariable("AuthProvider_AuthorizePath");
                opts.TokenPath = Environment.GetEnvironmentVariable("AuthProvider_TokenPath");
                opts.JwksPath = Environment.GetEnvironmentVariable("AuthProvider_JwksPath");
                opts.ChallengePhrase = Environment.GetEnvironmentVariable("AuthProvider_ChallengePhrase");
                opts.ClientId = Environment.GetEnvironmentVariable("AuthProvider_ClientId");
                opts.ReturnUri = Environment.GetEnvironmentVariable("AuthProvider_ReturnUri");
            })
            .AddScoped<IUserContextProvider, UserContextProvider>()
            .AddSingleton<IRequestAuthenticatorFactory, RequestAuthenticatorFactory>()
            .AddSingleton<BearerTokenRequestAuthenticator>()
            .AddSingleton<ApiKeyRequestAuthenticator>()
            .AddSingleton<IJwksRetriever, JwksRetriever>()
            .AddTransient<ISecurityTokenValidator, JwtSecurityTokenHandler>()            
            .AddMemoryCache();
        
        builder.Services
            .AddSingleton<IOpenApiConfigurationOptions>(_ => 
            {
                var options = new OpenApiConfigurationOptions()
                {
                    Info = new OpenApiInfo()
                    {
                        Version = "0.0.1",
                        Title = "NHS Appointment Book API",
                        Description = "NHS Appointment Book API",
                    },
                    Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                    OpenApiVersion = OpenApiVersionType.V2,
                    IncludeRequestingHostName = true,
                    ForceHttps = false,
                    ForceHttp = false,
                };
                return options;
            });

        builder.Services
            .Configure<CosmosDataStoreOptions>(opts => opts.DatabaseName = "appts")
            .Configure<ReferenceGroupOptions>(opts => opts.InitialGroupCount = 100)
            .AddTransient<ITemplateDocumentStore, WeekTemplateCosmosDocumentStore>()            
            .AddTransient<ISiteConfigurationStore, SiteConfigurationCosmosDocumentStore>()
            .AddTransient<IBookingsDocumentStore, BookingCosmosDocumentStore>()
            .AddTransient<IReferenceNumberDocumentStore, ReferenceGroupCosmosDocumentStore>()
            .AddTransient<IUserSiteAssignmentStore, UserSiteAssignmentStore>()
            .AddCosmosDataStores()
            .Configure<SiteSearchService.Options>(opts => opts.ServiceName = "APIM")
            .Configure<PostcodeLookupService.Options>(opts => opts.ServiceName = "APIM")
            .AddTransient<ITemplateService, TemplateService>()
            .AddTransient<IScheduleService, ScheduleService>()
            .AddTransient<IBookingsService, BookingsService>()
            .AddTransient<ISiteConfigurationService, SiteConfigurationService>()
            .AddTransient<IPostcodeLookupService, PostcodeLookupService>()
            .AddTransient<ISiteSearchService, SiteSearchService>()
            .AddTransient<IAvailabilityCalculator, AvailabilityCalculator>()
            .AddTransient<IAvailabilityGrouperFactory, AvailabilityGrouperFactory>()
            .AddTransient<IReferenceNumberProvider, ReferenceNumberProvider>()
            .AddTransient<IUserSiteAssignmentService, UserSiteAssignmentService>()
            .AddTransient<IDateTimeProvider, SystemDateTimeProvider>()
            .AddTransient<IPermissionChecker, PermissionChecker>()
            .AddAutoMapper(typeof(CosmosAutoMapperProfile));

    var leaseManagerConnection = Environment.GetEnvironmentVariable("LEASE_MANAGER_CONNECTION");
    if (leaseManagerConnection == "local")
        builder.Services.AddInMemoryLeasing();
    else
        builder.Services.AddAzureBlobStoreLeasing(leaseManagerConnection, "leases");

    builder.Services.AddHttpClient("APIM", builder =>
    {
        builder.BaseAddress = new Uri(Environment.GetEnvironmentVariable("APIM_ENDPOINT"));
        var subscriptionKey = Environment.GetEnvironmentVariable("APIM_SUBSCRIPTION_KEY");
        if (subscriptionKey is not null)
        {
            builder.DefaultRequestHeaders.Add("Subscription-Key", subscriptionKey);
        }
    });

    var cosmosEndpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT", EnvironmentVariableTarget.Process);
    var cosmosToken = Environment.GetEnvironmentVariable("COSMOS_TOKEN", EnvironmentVariableTarget.Process);
    var cosmosOptions = GetCosmosOptions(cosmosEndpoint);

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
    await database.Database.CreateContainerIfNotExistsAsync(id: "index_data", partitionKeyPath: "/docType");
}

private static CosmosClientOptions GetCosmosOptions(string cosmosEndpoint)
{
    if(cosmosEndpoint.StartsWith("https://localhost"))
    {
        return new CosmosClientOptions()
        {
            HttpClientFactory = () => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }),                    
            Serializer = new CosmosJsonSerializer(),
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true
        };                
    }

    return new()
    {
        Serializer = new CosmosJsonSerializer()
    };            
}
}