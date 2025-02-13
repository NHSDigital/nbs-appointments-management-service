using BookingsDataExtracts;
using DataExtract;

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
    .AddExtractWorker<BookingDataExtract>();

var host = builder.Build();
host.Run();
