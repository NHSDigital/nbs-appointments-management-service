using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Audit.Services;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Audit;

public static class ConfigurationExtensions
{
    public static IFunctionsWorkerApplicationBuilder AddAudit(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseMiddleware<Middleware>();
        builder.Services
            .AddSingleton<ITypedDocumentCosmosStore<AuditFunctionDocument>,
                TypedDocumentCosmosStore<AuditFunctionDocument>>();
        builder.Services
            .AddSingleton<ITypedDocumentCosmosStore<AuditAuthDocument>,
                TypedDocumentCosmosStore<AuditAuthDocument>>();
        builder.Services
            .AddTransient<ITypedDocumentCosmosStore<AuditNotificationDocument>,
                TypedDocumentCosmosStore<AuditNotificationDocument>>();
        builder.Services.AddSingleton<IAuditWriteService, AuditWriteService>();

        return builder;
    }
}
