using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using AuthorizationLevel = Microsoft.Azure.Functions.Worker.AuthorizationLevel;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core;
using System.Security.Claims;
using System.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Api.Functions;

public class GetAuthTokenFunction(IHttpClientFactory httpClientFactory, IOptions<AuthOptions> authOptions, ILogger<GetAuthTokenFunction> logger)
{
    private readonly AuthOptions _authOptions = authOptions.Value;

    [Function("GetAuthTokenFunction")]
    [AllowAnonymous]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "token")] HttpRequest req)
    {
        // need to resolve the correct auth config here
        var providerName = req.Query["provider"];
        var authProvider = authOptions.Value.Providers.Single(p => p.Name == providerName);

        var code = await req.ReadAsStringAsync();
        var formValues = new Dictionary<string, string>()
        {
            { "client_id", authProvider.ClientId },
            { "code", code },
            { "redirect_uri", authProvider.ReturnUri },
            { "scope", "openid profile email" },
            { "grant_type", "authorization_code" },
            { "code_verifier", authProvider.ChallengePhrase }
        };

        if (!string.IsNullOrEmpty(authProvider.ClientSecret))
        {
            formValues.Add("client_secret", authProvider.ClientSecret);
        }

        var form = new FormUrlEncodedContent(formValues);
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync($"{authProvider.TokenUri}", form);
        var rawResponse = await response.Content.ReadAsStringAsync();       
        // Should probably do some exception handling and logging here at some point
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(rawResponse);
        return new OkObjectResult(new { token = tokenResponse.IdToken });
    }
}
