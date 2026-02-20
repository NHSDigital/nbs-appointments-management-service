using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Core.Logger;
using Nhs.Appointments.Jobs.Aggregator;
using Nhs.Appointments.Jobs.ChangeFeed;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
    
builder.UseAppointmentsSerilog();
var applicationName = builder.Configuration.GetValue<string>("Application_Name") ?? throw new NullReferenceException("Application_Name is required");

var containerConfiguration =
    new ContainerConfiguration
    {
        ContainerName = "booking_data", 
        LeaseContainerName = "booking_aggregation_lease", 
        PollingIntervalSeconds = builder.Configuration.GetValue<int>("ChangeFeedPollingIntervalSeconds")
    };

builder.Services
    .AddSingleton(TimeProvider.System)
    .AddCosmos(builder.Configuration)
    .AddAggregationDomain(applicationName)
    .AddSingleton<ICosmosTransaction, CosmosTransaction>()
    .Configure<CosmosTransactionOptions>(builder.Configuration.GetSection("CosmosTransactionOptions"))
    .AddChangeFeedSink<AggregateSiteSummaryEvent, AggregatorSink>()
    .AddDataFilter<JObject, AggregateSiteSummaryEventDataFilter>()
    .AddFeedEventMapper<JObject, AggregateSiteSummaryEvent, AggregateSiteSummaryEventFeedEventMapper>()
    .AddDefaultChangeFeed<JObject, AggregateSiteSummaryEvent>([
        containerConfiguration
    ], applicationName)
    .AddChangeFeedWorker<JObject>(containerConfiguration)
    .Configure<DataFilterOptions>(builder.Configuration.GetSection("DataFilterOptions"));

var host = builder.Build();
host.Run();
