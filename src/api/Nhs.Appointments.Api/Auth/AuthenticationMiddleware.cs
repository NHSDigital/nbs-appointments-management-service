using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth;

public class AuthenticationMiddleware(IRequestAuthenticatorFactory requestAuthenticatorFactory) : IFunctionsWorkerMiddleware
{
    private readonly IRequestAuthenticatorFactory _requestAuthenticatorFactory = requestAuthenticatorFactory;
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var functionTypeInfoFeature = context.Features.Get<IFunctionTypeInfoFeature>();
        if(functionTypeInfoFeature.RequiresAuthentication == false)
        {
            await next(context);
            return;
        }

        var userContextProvider = context.InstanceServices.GetRequiredService<IUserContextProvider>() as UserContextProvider;
        var request = await context.GetHttpRequestDataAsync();
            
        if (request.Headers.Contains("Authorization"))
        {
            var authHeaderValue = request.Headers.GetValues("Authorization").FirstOrDefault();
            var parts = authHeaderValue.Split(' ');

            if (parts.Length == 2)
            {
                var scheme = parts[0];
                var value = parts[1];

                try
                {
                    var authenticator = _requestAuthenticatorFactory.CreateAuthenticator(scheme);
                    var principal = await authenticator.AuthenticateRequest(value);

                    if (principal.Identity.IsAuthenticated)
                    {
                        userContextProvider.UserPrincipal = principal;
                        await next(context);
                    }
                }
                catch (NotSupportedException)
                {

                }
            }
        }

        if (userContextProvider.UserPrincipal == null)
        {
            HandleUnauthorizedAccess(context);
        }
    }

    protected virtual void HandleUnauthorizedAccess(FunctionContext context)
    {
        var resultBinding = context.GetOutputBindings<IActionResult>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name == "$return");
        resultBinding.Value = JsonResponseWriter.WriteResult(null, System.Net.HttpStatusCode.Unauthorized);
    }
}

