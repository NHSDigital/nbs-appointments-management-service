using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace Nhs.Appointments.Audit.Functions;

public static class ConfigurationExtensions
{
    public static IFunctionsWorkerApplicationBuilder AddAudit(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseMiddleware<Middleware>();
        
        return builder;
    }
}
