using CosmosAuditor;
using CosmosAuditor.AuditSinks;
using CosmosAuditor.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Logging.AddConsole();

builder.Services
    .AddTransient<IAuditSink, ConsoleAuditSink>()
    .AddTransient<IAuditSink, BlobAuditSink>()
    .AddCosmos(builder.Configuration)
    .AddAzureBlobStorage(builder.Configuration)
    .AddAuditWorker<AuditContainerConfig>()
    .AddAuditWorker<BookingContainerConfig>()
    .AddAuditWorker<CoreContainerConfig>()
    .AddAuditWorker<IndexContainerConfig>();

var host = builder.Build();
host.Run();
