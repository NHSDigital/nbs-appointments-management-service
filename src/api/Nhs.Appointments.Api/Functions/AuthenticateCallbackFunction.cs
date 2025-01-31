using System;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class AuthenticateCallbackFunction(IOptions<AuthOptions> authOptions)
{
    [Function("AuthenticateCallbackFunction")]
    [AllowAnonymous]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth-return")]
        HttpRequest req)
    {
        
        var code = req.Query["code"];
        var redirectUri = req.Query["state"];
        var providerName = req.Query["provider"];
        var authProvider = string.IsNullOrWhiteSpace(providerName) ? authOptions.Value.Providers[0] : authOptions.Value.Providers.Single(p => p.Name == providerName);

        var redirectUrl = new UriBuilder(authProvider.ClientCodeExchangeUri);
        var query = HttpUtility.ParseQueryString(redirectUrl.Query);
        query["code"] = code;
        redirectUrl.Query = query.ToString();

        return new RedirectResult(redirectUrl.ToString());
    }
}
