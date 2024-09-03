using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Api.Auth;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Api.Functions;

public class AuthenticateFunction
{
    private readonly AuthOptions _authOptions;

    public AuthenticateFunction(IOptions<AuthOptions> authOptions)
    {
        _authOptions = authOptions.Value;
    }

    [Function("AuthenticateFunction")]
    [AllowAnonymous]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authenticate")] HttpRequest req)
    {
        var redirectUri = req.Query["redirect_uri"];
        var cc = GenerateCodeChallenge(_authOptions.ChallengePhrase);
        var queryStringValues = new Dictionary<string, string>
        {
            { "client_id",  _authOptions.ClientId},
            { "redirect_uri", _authOptions.ReturnUri },
            { "response_type", "code" },
            { "response_mode", "query" },
            { "code_challenge_method", "S256" },
            { "code_challenge", cc },
            { "scope", "openid profile email" },
            { "prompt", "login" },
            { "state", redirectUri },
        };

        var oidcAuthorizeUrl = QueryHelpers.AddQueryString($"{_authOptions.AuthorizeUri}", queryStringValues);
        return new RedirectResult(oidcAuthorizeUrl);
    }

    protected virtual string GenerateCodeChallenge(string challengePhrase) => AuthUtilities.GenerateCodeChallenge(_authOptions.ChallengePhrase);    
}
