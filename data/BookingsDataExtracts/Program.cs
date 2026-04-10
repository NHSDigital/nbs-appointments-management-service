using BookingsDataExtracts;
using DataExtract;
using DataExtract.Documents;
using Microsoft.FeatureManagement;
using Nhs.Appointments.Core.Logger;
using Nhs.Appointments.Persistance.Models;

var builder = Host.CreateApplicationBuilder(args);
var azureAppConfigConnection = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .AddNbsAzureKeyVault();

// Handle Feature Flag (Local vs Cloud)
if (azureAppConfigConnection == "local")
{
    var configPath = Path.Combine(AppContext.BaseDirectory, "local.feature.flags.json");
    builder.Configuration.AddJsonFile(configPath, optional: true, reloadOnChange: false);
}
else
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(azureAppConfigConnection)
               .UseFeatureFlags();
    });
}

builder.UseAppointmentsSerilog();

builder.Services
    .AddDataExtractServices("booking", builder.Configuration)
    .AddCosmosStore<NbsBookingDocument>()
    .AddCosmosStore<SiteDocument>()
    .AddExtractWorker<BookingDataExtract>()
    .AddFeatureManagement();

var host = builder.Build();
host.Run();
