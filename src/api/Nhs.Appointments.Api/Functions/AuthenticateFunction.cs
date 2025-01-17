using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Functions;

public class AuthenticateFunction(IOptions<AuthOptions> authOptions)
{
    [Function("AuthenticateFunction")]
    [AllowAnonymous]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authenticate")]
        HttpRequest req)
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

    protected virtual string GenerateCodeChallenge(string challengePhrase) =>
        AuthUtilities.GenerateCodeChallenge(authOptions.Value.ChallengePhrase);
}
