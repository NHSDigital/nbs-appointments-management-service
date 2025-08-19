using CapacityDataExtracts;
using DataExtract;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nbs.MeshClient;
using Nhs.Appointments.Persistance.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddNbsAzureKeyVault();

builder.Services
    .AddDataExtractServices("BookingCapacity", builder.Configuration, args.Contains("create-local-sample"))
    .AddCosmosStore<DailyAvailabilityDocument>()
    .AddCosmosStore<SiteDocument>()
    .AddExtractWorker<CapacityDataExtract>()
    .Configure<FileSenderOptions>(
        builder.Configuration.GetSection("FileSenderOptions"))
    .Configure<MeshFileOptions>(
        builder.Configuration.GetSection("MeshFileOptions"))
    .Configure<LocalFileOptions>(
        builder.Configuration.GetSection("LocalFileOptions"))
    .Configure<BlobFileOptions>(
        builder.Configuration.GetSection("BlobFileOptions"))
    .AddScoped<IMeshMailbox>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<MeshMailbox>>();
        var client = sp.GetRequiredService<IMeshClient>();
        var mailboxId = builder.Configuration["FileSenderOptions:Mesh:DestinationMailboxId"];
        return new MeshMailbox(mailboxId, logger, client);
    })
    .AddScoped<IFileSenderFactory, FileSenderFactory>()
    .AddAzureClients(x =>
    {
        x.AddBlobServiceClient(builder.Configuration["BlobStorageConnectionString"]);
    });

var host = builder.Build();
host.Run();
