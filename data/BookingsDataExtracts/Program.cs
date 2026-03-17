using BookingsDataExtracts;
using DataExtract;
using DataExtract.Documents;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Nhs.Appointments.Persistance.Models;
using Nhs.Appointments.Api.Features; // LocalConfigurationRefresher


var builder = Host.CreateApplicationBuilder(args);
var azureAppConfigConnection = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");

IConfigurationRefresher? configRefresher = null;

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .AddNbsAzureKeyVault();

// Handle Feature Flag (Local vs Cloud)
if (azureAppConfigConnection == "local")
{
    configRefresher = new LocalConfigurationRefresher();
    var configPath = Path.Combine(AppContext.BaseDirectory, "local.feature.flags.json");
    builder.Configuration.AddJsonFile(configPath, optional: false, reloadOnChange: true);
}
else
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(azureAppConfigConnection)
               .UseFeatureFlags()
               .ConfigureRefresh(refresh =>
               {
                   refresh.RegisterAll().SetRefreshInterval(TimeSpan.FromMinutes(10));
               });

        configRefresher = options.GetRefresher();
    });
}

builder.Logging.AddConsole();

builder.Services
    .AddDataExtractServices("booking", builder.Configuration)
    .AddCosmosStore<NbsBookingDocument>()
    .AddCosmosStore<SiteDocument>()
    .AddExtractWorker<BookingDataExtract>()
    .AddFeatureManagement();

if (configRefresher != null)
{
    builder.Services.AddSingleton(configRefresher);
}

builder.Services.AddSingleton<ITargetingContextAccessor, DefaultContextAccessor>();

var host = builder.Build();
host.Run();
