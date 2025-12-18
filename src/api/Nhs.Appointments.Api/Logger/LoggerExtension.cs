using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Nhs.Appointments.Api.Logger;

public static class LoggerExtension
{
    public static IHostBuilder UseAppointmentsSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.SetupDefaultLoggerConfiguration();
        });
    }

    public static IHostApplicationBuilder UseAppointmentsSerilog(this IHostApplicationBuilder builder)
    {
        var logger = new LoggerConfiguration()
            .SetupDefaultLoggerConfiguration()
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger, dispose: true);

        return builder;
    }
}
