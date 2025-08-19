using BookingsDataExtracts;
using DataExtract;
using DataExtract.Documents;
using Microsoft.Extensions.Azure;
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
    .AddDataExtractServices("booking", builder.Configuration, args.Contains("create-local-sample"))
    .AddCosmosStore<NbsBookingDocument>()
    .AddCosmosStore<SiteDocument>()
    .AddExtractWorker<BookingDataExtract>()
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
