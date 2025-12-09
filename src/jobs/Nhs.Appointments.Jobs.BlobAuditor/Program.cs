using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor;
using Nhs.Appointments.Jobs.BlobAuditor.ChangeFeed;
using Nhs.Appointments.Jobs.BlobAuditor.Configuration;
using Nhs.Appointments.Jobs.BlobAuditor.Sink;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var mySettingsSection = builder.Configuration.GetSection("AuditWorkerConfigurations");
builder.Services.Configure<List<ContainerConfiguration>>(mySettingsSection);

builder.Logging.AddConsole();

builder.Services
    .AddSingleton(TimeProvider.System)
    .AddTransient<IBlobSink<JObject>, BlobSink>()
    .AddCosmos(builder.Configuration)
    .AddAzureBlobStorage(builder.Configuration)
    .AddTransient<IAuditChangeFeedHandler<JObject>, AuditChangeFeedHandler>()
    .AddTransient<IContainerConfigFactory, ContainerConfigFactory>()
    .AddDynamicallyConfiguredAuditWorkers<JObject>(builder.Configuration);

var host = builder.Build();
host.Run();
