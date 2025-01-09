using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Audit;

public class AuditMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var auditFunctionTypeInfoFeature = context.Features.Get<IAuditFunctionTypeInfoFeature>();
        var requiresAudit = auditFunctionTypeInfoFeature?.RequiresAudit ?? false;

        if (!requiresAudit)
        {
            await next(context);
            return;
        }

        var userContextProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>();

        var requestSiteInspectorType = auditFunctionTypeInfoFeature!.RequestSiteInspector;

        var userId = userContextProvider.UserPrincipal.Claims.GetUserEmail();
        var actionType = context.FunctionDefinition.Name;

        var siteId = string.Empty;
        if (requestSiteInspectorType is not null &&
            context.InstanceServices.GetService(requestSiteInspectorType) is IRequestInspector requestInspector)
        {
            var request = await context.GetHttpRequestDataAsync();

            //TODO not expecting multiple site matches in first usages
            siteId = (await requestInspector.GetSiteIds(request)).SingleOrDefault();
        }

        SaveToDb(siteId);
    }

    private void SaveToDb(string? siteId)
    {
    }
}
