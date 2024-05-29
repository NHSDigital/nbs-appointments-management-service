using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Api.Functions;

public static class AuthenticateCallbackFunction
{
    [Function("AuthenticateCallbackFunction")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth-return")] HttpRequest req)
    {
        var code = req.Query["code"];
        var redirectUri = req.Query["state"];        
        return new RedirectResult($"{redirectUri}?code={code}");
    }
}
