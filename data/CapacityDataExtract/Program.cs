using CapacityDataExtracts;
using DataExtract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Persistance.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddNbsAzureKeyVault();

builder.Services
    .AddDataExtractServices("capacity", builder.Configuration, args.Contains("create-local-sample"))
    .AddCosmosStore<DailyAvailabilityDocument>()
    .AddCosmosStore<SiteDocument>()
    .AddExtractWorker<CapacityDataExtract>();

var host = builder.Build();
host.Run();
