using Microsoft.Extensions.Configuration;
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

builder.Services
    .AddCosmos(builder.Configuration)
    .AddChangeFeedSink<AggregateSiteSummaryEvent, AggregatorSink>()
    .AddFeedEventMapper<JObject, AggregateSiteSummaryEvent, AggregateSiteSummaryEventFeedEventMapper>()
    .AddDefaultChangeFeed<JObject, AggregateSiteSummaryEvent>([
        new ContainerConfiguration { ContainerName = "booking_data", LeaseContainerName = "booking_aggregation_lease" }
    ], applicationName);
