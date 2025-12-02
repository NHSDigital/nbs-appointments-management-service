using BookingsDataExtracts;
using DataExtract;
using DataExtract.Documents;
using Nhs.Appointments.Persistance.Models;

var builder = Host.CreateApplicationBuilder(args);
var azureAppConfigConnection = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .AddNbsAzureKeyVault()
    .AddAzureAppConfiguration(options =>
    {
        options.Connect(azureAppConfigConnection)
            .UseFeatureFlags();
    });

builder.Logging.AddConsole();

builder.Services
    .AddDataExtractServices("booking", builder.Configuration)
    .AddCosmosStore<NbsBookingDocument>()
    .AddCosmosStore<SiteDocument>()
    .AddExtractWorker<BookingDataExtract>();

var host = builder.Build();
host.Run();
