using BookingsDataExtracts;
using BookingsDataExtracts.Documents;
using DataExtract;
using Nhs.Appointments.Persistance.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddNbsAzureKeyVault();

builder.Services
    .AddSingleton<BookingDataExtract>()
    .AddDataExtractServices(builder.Configuration)
    .AddCosmosStore<NbsBookingDocument>()
    .AddCosmosStore<SiteDocument>()
    .AddExtractWorker<BookingDataExtract>();

var host = builder.Build();
host.Run();
