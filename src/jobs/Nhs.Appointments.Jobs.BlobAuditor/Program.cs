using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor;
using Nhs.Appointments.Jobs.BlobAuditor.ChangeFeed;
using Nhs.Appointments.Jobs.BlobAuditor.Cosmos;
using Nhs.Appointments.Jobs.BlobAuditor.Sink;
using Nhs.Appointments.Jobs.BlobAuditor.Sink.Config;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var containerConfigs = builder.Configuration.GetSection("AuditWorkerConfigurations");
builder.Services.Configure<List<ContainerConfiguration>>(containerConfigs);

builder.Services.Configure<List<SinkExclusion>>(
    builder.Configuration.GetSection("SinkExclusions"));

builder.Logging.AddConsole();

builder.Services
    .AddSingleton(TimeProvider.System)
    .AddSingleton<IItemExclusionProcessor, ItemExclusionProcessor>()
    .AddTransient<IBlobSink<JObject>, BlobSink>()
    .AddCosmos(builder.Configuration)
    .AddAzureBlobStorage(builder.Configuration)
    .AddTransient<IAuditChangeFeedHandler<JObject>, AuditChangeFeedHandler>()
    .AddTransient<IContainerConfigFactory, ContainerConfigFactory>();

foreach (var config in containerConfigs.Get<List<ContainerConfiguration>>()!)
{
    builder.Services.AddAuditWorker<JObject>(config);
}

var host = builder.Build();
host.Run();
