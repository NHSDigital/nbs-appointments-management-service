using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nhs.Appointments.Api.Json;

namespace Nhs.Appointments.Api.Auth;

public class AuthorizationMiddleware(IPermissionChecker permissionChecker) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var functionTypeInfoFeature = context.Features.Get<IFunctionTypeInfoFeature>();
        var requiredPermission = functionTypeInfoFeature.RequiredPermission;
        if(requiredPermission.IsNullOrEmpty() || await IsAuthorized(context, requiredPermission))
        {
            await next(context);
            return;
        }
        HandleUnauthorizedAccess(context);
    }

    private Task<bool> IsAuthorized(FunctionContext context, string requiredPermission)
    {
        var userContextProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>();
        var siteId = string.Empty;
        if (context.Items.TryGetValue("siteId", out var siteIdValue))
        {
            siteId = siteIdValue.ToString();
        }
        var userEmail = userContextProvider.UserPrincipal.Claims.GetUserEmail();
        return permissionChecker.HasPermissionAsync(userEmail, siteId, requiredPermission);
    }
    
    protected virtual void HandleUnauthorizedAccess(FunctionContext context)
    {
        var resultBinding = context.GetOutputBindings<IActionResult>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name == "$return");
        resultBinding.Value = JsonResponseWriter.WriteResult(null, HttpStatusCode.Forbidden);
    }
}
