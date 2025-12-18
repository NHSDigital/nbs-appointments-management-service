using Nhs.Appointments.Core.Logger.Masks;
using Serilog;
using Serilog.Enrichers.Sensitive;

namespace Nhs.Appointments.Core.Logger;

internal static class SerilogSetupExtension
{
    internal static LoggerConfiguration SetupDefaultLoggerConfiguration(this LoggerConfiguration loggerConfiguration)
    {
        var (splunkHost, eventCollectorToken) = GetSplunkEventCollectorConfig();

        loggerConfiguration = loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithSensitiveDataMasking(options =>
            {
                options.ExcludeProperties = new List<string>
                {
                    // Prevent GUID and other hexadecimal-based properties from being masked.
                    // NHS Number mask will sometimes mask parts of them if not excluded.
                    "ActionId",
                    "CorrelationId",
                    "RequestId",
                    "SpanId",
                    "TraceId",
                    "NhsNumberHash",
                    nameof(Environment.MachineName),
                };
                options.MaskingOperators = new List<IMaskingOperator>
                {
                    new EmailAddressMaskingOperator(),
                    new NhsNumberMaskingOperator(),
                };
            })
            .WriteTo.Console()
            .WriteTo.EventCollector(
                splunkHost,
                eventCollectorToken,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
            );

        return loggerConfiguration;
    }

    private static (string splunkHost, string eventCollectorToken) GetSplunkEventCollectorConfig()
    {
        var splunkHost = Environment.GetEnvironmentVariable("SPLUNK_HOST_URL");
        var eventCollectorToken = Environment.GetEnvironmentVariable("SPLUNK_HEC_TOKEN");

        return (splunkHost, eventCollectorToken);
    }
}
