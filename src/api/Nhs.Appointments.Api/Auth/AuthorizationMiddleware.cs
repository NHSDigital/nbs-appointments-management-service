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
        var userContextProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>();
        var userEmail = userContextProvider.UserPrincipal.Claims.GetUserEmail();
        
        var functionTypeInfoFeature = context.Features.Get<IFunctionTypeInfoFeature>();
        var requiredPermission = functionTypeInfoFeature.RequiredPermission;
        Task<bool> IsAuthorized() => permissionChecker.HasPermissionAsync(userEmail, requiredPermission);
        if(requiredPermission.IsNullOrEmpty() || await IsAuthorized())
        {
            await next(context);
            return;
        }
        HandleUnauthorizedAccess(context);
    }
    
    protected virtual void HandleUnauthorizedAccess(FunctionContext context)
    {
        var resultBinding = context.GetOutputBindings<IActionResult>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name == "$return");
        resultBinding.Value = JsonResponseWriter.WriteResult(null, HttpStatusCode.Forbidden);
    }
}
