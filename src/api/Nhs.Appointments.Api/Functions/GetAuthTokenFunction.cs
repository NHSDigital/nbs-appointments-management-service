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
using System;


namespace Nhs.Appointments.Api.Functions;

public class GetAuthTokenFunction(IHttpClientFactory httpClientFactory, IOptions<AuthOptions> authOptions)
{
    private readonly AuthOptions _authOptions = authOptions.Value;

    [Function("GetAuthTokenFunction")]
    [AllowAnonymous]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "token")] HttpRequest req)
    {
        var code = await req.ReadAsStringAsync();
        var formValues = new Dictionary<string, string>()
        {
            { "client_id", _authOptions.ClientId },
            { "code", code },
            { "redirect_uri", _authOptions.ReturnUri },
            { "scope", "openid profile email" },
            { "grant_type", "authorization_code" },
            { "code_verifier", _authOptions.ChallengePhrase }
        };

        var form = new FormUrlEncodedContent(formValues);
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync($"{_authOptions.TokenUri}", form);
        Console.WriteLine(response.StatusCode);
        var rawResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine(rawResponse);
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(rawResponse);
        return new OkObjectResult(new { token = tokenResponse.IdToken });
    }
}
