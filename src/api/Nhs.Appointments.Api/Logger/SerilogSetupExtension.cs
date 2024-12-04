using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace Nhs.Appointments.Api.Logger
{
    internal static class SerilogSetupExtension
    {
        internal static LoggerConfiguration SetupDefaultLoggerConfiguration(this LoggerConfiguration loggerConfiguration, HostBuilderContext hostingContext)
        {
            var (splunkHost, eventCollectorToken) = GetSplunkEventCollectorConfig(hostingContext.Configuration);

            loggerConfiguration = loggerConfiguration
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.EventCollector(
                    splunkHost,
                    eventCollectorToken,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
                );

            return loggerConfiguration;
        }

        private static (string splunkHost, string eventCollectorToken) GetSplunkEventCollectorConfig(IConfiguration config)
        {
            var splunkHost = Environment.GetEnvironmentVariable("SPLUNK_HOST_URL");
            var eventCollectorToken = Environment.GetEnvironmentVariable("SPLUNK_HEC_TOKEN");

            return (splunkHost, eventCollectorToken);
        }
    }
}
