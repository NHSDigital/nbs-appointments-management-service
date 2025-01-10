using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Api.Auth;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Audit;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

public class AuthenticateFunction(IOptions<AuthOptions> authOptions)
{    
    [Function("AuthenticateFunction")]
    [RequiresAudit(typeof(NoSiteRequestInspector))]
    [AllowAnonymous]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authenticate")] HttpRequest req)
    {
        var cc = GenerateCodeChallenge(authOptions.Value.ChallengePhrase);
        var queryStringValues = new Dictionary<string, string>
        {
            { "client_id", authOptions.Value.ClientId },
            { "redirect_uri", authOptions.Value.ReturnUri },
            { "response_type", "code" },
            { "response_mode", "query" },
            { "code_challenge_method", "S256" },
            { "code_challenge", cc },
            { "scope", "openid profile email" },
            { "prompt", "login" },
        };

        var oidcAuthorizeUrl = QueryHelpers.AddQueryString($"{authOptions.Value.AuthorizeUri}", queryStringValues);
        return new RedirectResult(oidcAuthorizeUrl);
    }

    protected virtual string GenerateCodeChallenge(string challengePhrase) => AuthUtilities.GenerateCodeChallenge(authOptions.Value.ChallengePhrase);    
}
