using BookingsDataExtracts;
using DataExtract;
using DataExtract.Documents;
using Nhs.Appointments.Persistance.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddNbsAzureKeyVault();

builder.Services
    .AddDataExtractServices("booking", builder.Configuration, args.Contains("create-local-sample"))
    .AddCosmosStore<NbsBookingDocument>()
    .AddCosmosStore<SiteDocument>()
    .AddExtractWorker<BookingDataExtract>();

var host = builder.Build();
host.Run();
