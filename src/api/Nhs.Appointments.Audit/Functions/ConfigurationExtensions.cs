using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Audit.Persistance;

namespace Nhs.Appointments.Audit.Functions;

public static class ConfigurationExtensions
{
    public static IFunctionsWorkerApplicationBuilder AddAudit(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseMiddleware<Middleware>();
        builder.Services.AddTransient<IAuditDocumentStore, AuditCosmosDocumentStore>();
        
        return builder;
    }
}
