using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Nhs.Appointments.Api.Functions;

public static class AuthenticateCallbackFunction
{
    [Function("AuthenticateCallbackFunction")]
    [AllowAnonymous]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth-return")] HttpRequest req)
    {
        var code = req.Query["code"];
        var redirectUri = req.Query["state"].First();
        var sep = redirectUri.Contains("?") ? "&" : "?";
        return new RedirectResult($"{redirectUri}{sep}code={code}");
    }
}
