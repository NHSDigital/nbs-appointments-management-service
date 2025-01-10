using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Audit.Functions;

public class Middleware(IAuditDocumentStore auditDocumentStore) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requiresAudit = GetRequiresAudit(context);

        if (requiresAudit == null)
        {
            await next(context);
            return;
        }

        _ = Task.Run(() => RecordAudit(context, requiresAudit));

        await next(context);
    }

    private async Task<AuditFunctionDocument> BuildAuditFunctionDocument(FunctionContext context,
        RequiresAuditAttribute requiresAudit)
    {
        var userContextProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>();
        var requestSiteInspectorType = requiresAudit.RequestSiteInspector;
        var userId = userContextProvider.UserPrincipal?.Claims.GetUserEmail();
        var functionName = context.FunctionDefinition.Name;
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

        return new AuditFunctionDocument
        {
            Timestamp = DateTime.UtcNow, UserId = userId, ActionType = functionName, SiteId = siteId
        };
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

    private async Task RecordAudit(FunctionContext context, RequiresAuditAttribute requiresAudit)
    {
        var auditFunctionDocument = await BuildAuditFunctionDocument(context, requiresAudit);
        await auditDocumentStore.InsertAsync(auditFunctionDocument);
    }
}
