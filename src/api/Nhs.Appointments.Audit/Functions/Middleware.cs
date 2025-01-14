using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Audit.Services;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Audit.Functions;

public class Middleware(IAuditWriteService auditWriteService) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requiresAudit = GetRequiresAudit(context);

        if (requiresAudit == null)
        {
            await next(context);
            return;
        }

        var auditData = await ExtractAuditData(context, requiresAudit.RequestSiteInspector);
        _ = Task.Run(() => auditWriteService.RecordFunction(auditData.id, DateTime.UtcNow, auditData.user, auditData.function, auditData.site));

        await next(context);
    }

    private static async Task<(string id, string user, string function, string site)> ExtractAuditData(FunctionContext context, Type requestSiteInspectorType)
    {
        var site = string.Empty;
        if (context.InstanceServices.GetService(requestSiteInspectorType) is IRequestInspector requestInspector)
        {
            var request = await context.GetHttpRequestDataAsync();
            var sites = (await requestInspector.GetSiteIds(request)).ToList();

            if (sites.Count > 1)
            {
                throw new NotImplementedException("Auditing a function with multiple sites is not currently supported");
            }
            
            site = sites.SingleOrDefault();
        }
        
        var userProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>();
        var user = userProvider.UserPrincipal?.Claims.GetUserEmail();
        var functionName = context.FunctionDefinition.Name;
        var functionAuditId = $"{context.FunctionId}_{context.InvocationId}";

        return (functionAuditId, user, functionName, site);
    }

    private RequiresAuditAttribute GetRequiresAudit(FunctionContext context)
    {
        var assembly = Assembly.LoadFrom(context.FunctionDefinition.PathToAssembly);
        var typeName = string.Join(".", context.FunctionDefinition.EntryPoint.Split('.')[..^1]);
        var methodName = context.FunctionDefinition.EntryPoint.Split('.')[^1];

        var type = assembly.GetType(typeName);
        var methodInfo = type?.GetMethod(methodName);

        return methodInfo?.GetCustomAttribute<RequiresAuditAttribute>();
    }
}
