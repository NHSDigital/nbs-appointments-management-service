using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Core.Logger;
using Nhs.Appointments.Jobs.BlobAuditor;
using Nhs.Appointments.Jobs.BlobAuditor.Sink;
using Nhs.Appointments.Jobs.BlobAuditor.Sink.Config;
using Nhs.Appointments.Jobs.ChangeFeed;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<List<SinkExclusion>>(
    builder.Configuration.GetSection("SinkExclusions"));

var containerConfigs = builder.Configuration.GetSection("AuditWorkerConfigurations").Get<List<ContainerConfiguration>>()!;
var applicationName = builder.Configuration.GetValue<string>("Application_Name") ?? throw new NullReferenceException("Application_Name is required");

builder.UseAppointmentsSerilog();

builder.Services
    .AddSingleton(TimeProvider.System)
    .AddSingleton<IItemExclusionProcessor, ItemExclusionProcessor>()
    .AddCosmos(builder.Configuration)
    .AddAzureBlobStorage(builder.Configuration)
    .AddChangeFeedSink<JObject, BlobSink>()
    .AddDataFilter<JObject, AuditDataFilter>()
    .AddFeedEventMapper<JObject, JObject, AuditFeedEventMapper>()
    .AddDefaultChangeFeed<JObject, JObject>(containerConfigs, applicationName)
    .Configure<DataFilterOptions>(builder.Configuration.GetSection("DataFilterOptions"));

foreach (var config in containerConfigs)
{
    builder.Services.AddChangeFeedWorker<JObject>(config);
}

var host = builder.Build();
host.Run();
