using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Nhs.Appointments.Api.Logger
{
    public static class LoggerExtension
    {
        public static IHostBuilder UseAppointmentsSerilog(this IHostBuilder builder)
        {
            return builder.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration.SetupDefaultLoggerConfiguration(hostingContext);
            });
        }
    }
}
