using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Json;


namespace Nhs.Appointments.Api.Auth;

public class AuthorizationMiddleware(IPermissionChecker permissionChecker) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var functionTypeInfoFeature = context.Features.Get<IFunctionTypeInfoFeature>();
        var requiredPermission = functionTypeInfoFeature.RequiredPermission;
        var requestInspectorType = functionTypeInfoFeature.RequestInspector;
        if(string.IsNullOrEmpty(requiredPermission) || await IsAuthorized(context, requiredPermission, requestInspectorType))
        {
            await next(context);
            return;
        }
        HandleUnauthorizedAccess(context);
    }

    private async Task<bool> IsAuthorized(FunctionContext context, string requiredPermission, Type requestInspectorType)
    {
        var userContextProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>();
        
        var siteId = string.Empty;
        if(requestInspectorType is not null)
        {
            var requestInspector = context.InstanceServices.GetService(requestInspectorType) as IRequestInspector;
            if (requestInspector is not null)
            {
                var request = await context.GetHttpRequestDataAsync();
                siteId = await requestInspector.GetSiteId(request);
            }
        }
        
        var userEmail = userContextProvider.UserPrincipal.Claims.GetUserEmail();
        return await permissionChecker.HasPermissionAsync(userEmail, siteId, requiredPermission);
    }
    
    protected virtual void HandleUnauthorizedAccess(FunctionContext context)
    {
        var resultBinding = context.GetOutputBindings<IActionResult>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name == "$return");
        resultBinding.Value = JsonResponseWriter.WriteResult(null, HttpStatusCode.Forbidden);
    }
}
