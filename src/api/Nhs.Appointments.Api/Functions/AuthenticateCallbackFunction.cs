using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
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
        return new RedirectResult($"{authOptions.Value.ClientCodeExchangeUri}?code={code}");
    }
}
