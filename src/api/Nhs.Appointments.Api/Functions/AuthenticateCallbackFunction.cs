using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Api.Auth;
using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Api.Functions;

public class AuthenticateCallbackFunction(IOptions<AuthOptions> authOptions)
{
    [Function("AuthenticateCallbackFunction")]
    [AllowAnonymous]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth-return")] HttpRequest req)
    {
        var code = req.Query["code"];
        var redirectUri = req.Query["state"];        
        return new RedirectResult($"{authOptions.Value.ClientCodeExchangeUri}?code={code}");
    }
}
