using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Audit.Functions;

public static class ConfigurationExtensions
{
    public static IFunctionsWorkerApplicationBuilder AddAudit(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseMiddleware<Middleware>();
        builder.Services.AddTransient<IAuditDocumentStore, AuditCosmosDocumentStore>();
        //
        // var serviceType = typeof(ITypedDocumentCosmosStore<>).MakeGenericType(typeof(AuditFunctionDocument));
        // var implementationType = typeof(TypedDocumentCosmosStore<>).MakeGenericType(typeof(AuditFunctionDocument));
        builder.Services.AddTransient(typeof(ITypedDocumentCosmosStore<AuditFunctionDocument>), typeof(TypedDocumentCosmosStore<AuditFunctionDocument>));
        
        return builder;
    }
}
