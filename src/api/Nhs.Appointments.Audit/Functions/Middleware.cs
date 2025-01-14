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

        var siteId = await ExtractSiteId(context, requiresAudit.RequestSiteInspector);
        _ = Task.Run(() => RecordAudit(context, siteId));

        await next(context);
    }

    private static async Task<string> ExtractSiteId(FunctionContext context, Type requestSiteInspectorType)
    {
        var siteId = string.Empty;
        if (context.InstanceServices.GetService(requestSiteInspectorType) is IRequestInspector requestInspector)
        {
            var request = await context.GetHttpRequestDataAsync();
            var sites = (await requestInspector.GetSiteIds(request)).ToList();

            if (sites.Count > 1)
            {
                throw new NotImplementedException("Auditing a function with multiple sites is not currently supported");
            }
            
            siteId = sites.SingleOrDefault();
        }

        return siteId;
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

    private async Task RecordAudit(FunctionContext context, string siteId)
    {
        var userProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>();
        var userId = userProvider.UserPrincipal?.Claims.GetUserEmail();
        var functionName = context.FunctionDefinition.Name;
        var functionAuditId = $"{context.FunctionId}_{context.InvocationId}";

        await auditWriteService.RecordFunction(functionAuditId, DateTime.UtcNow, userId, functionName, siteId);
    }
}
