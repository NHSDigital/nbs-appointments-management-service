using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Api.Auth;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Nhs.Appointments.Api.Functions;

public class AuthenticateFunction(IOptions<AuthOptions> authOptions)
{    
    [Function("AuthenticateFunction")]
    [AllowAnonymous]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authenticate")] HttpRequest req)
    {
        // here we need to resolve the correct auth provider
        var providerName = req.Query["provider"];
        var authProvider = authOptions.Value.Providers.Single(p => p.Name == providerName);

        var cc = AuthUtilities.GenerateCodeChallenge(authProvider.ChallengePhrase);

        var queryStringValues = new Dictionary<string, string>
        {
            { "client_id",  authProvider.ClientId},
            { "redirect_uri", authProvider.ReturnUri },
            { "response_type", "code" },
            { "response_mode", "query" },
            { "code_challenge_method", "S256" },
            { "code_challenge", cc },
            { "scope", "openid profile email" },
            { "prompt", "login" },
        };

        if (authProvider.RequiresStateForAuthorize)
        {
            queryStringValues.Add("state", Guid.NewGuid().ToString());
        }

        var oidcAuthorizeUrl = QueryHelpers.AddQueryString($"{authProvider.AuthorizeUri}", queryStringValues);
        return new RedirectResult(oidcAuthorizeUrl);
    }
}
