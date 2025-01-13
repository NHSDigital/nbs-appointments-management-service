using BookingsDataExtracts;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddNbsAzureKeyVault();            

builder.Services
    .AddDataExtractServices(builder.Configuration)
    .AddHostedService<DataExtractWorker>();

var host = builder.Build();
host.Run();
