using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Audit.Functions;

public class Middleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requiresAudit = GetRequiresAudit(context);

        if (requiresAudit == null)
        {
            await next(context);
            return;
        }
        
        //TODO make sure not awaiting model being built as part of fire and forget?
        var audit = await BuildAuditModel(context, requiresAudit);

        _ = RecordAudit(audit);
        
        await next(context);
    }

    private async Task<Models.Audit> BuildAuditModel(FunctionContext context, RequiresAuditAttribute requiresAudit)
    {
        var userContextProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>();
        var requestSiteInspectorType = requiresAudit.RequestSiteInspector;
        var userId = userContextProvider.UserPrincipal?.Claims.GetUserEmail();
        var actionType = context.FunctionDefinition.Name;
        var siteId = string.Empty;
        if (context.InstanceServices.GetService(requestSiteInspectorType) is IRequestInspector requestInspector)
        {
            var request = await context.GetHttpRequestDataAsync();

            //TODO not expecting multiple site matches in first usages??
            siteId = (await requestInspector.GetSiteIds(request)).SingleOrDefault();
        }
        
        //TODO get eventId from somewhere?
        return new Models.Audit(DateTime.UtcNow, userId, actionType, siteId);
    }

    private RequiresAuditAttribute? GetRequiresAudit(FunctionContext context)
    {
        var assembly = Assembly.LoadFrom(context.FunctionDefinition.PathToAssembly);
        var typeName = string.Join(".", context.FunctionDefinition.EntryPoint.Split('.')[..^1]);
        var methodName = context.FunctionDefinition.EntryPoint.Split('.')[^1];

        var type = assembly.GetType(typeName);
        var methodInfo = type?.GetMethod(methodName);

        return methodInfo?.GetCustomAttribute<RequiresAuditAttribute>();
    }

    protected virtual async Task RecordAudit(Models.Audit auditToWrite)
    {
        //TODO fire and forget to CosmosDB with structured audit record?
    }
}
