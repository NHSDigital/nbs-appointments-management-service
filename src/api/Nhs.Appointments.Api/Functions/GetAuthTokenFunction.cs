using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Audit.Services;
using AuthorizationLevel = Microsoft.Azure.Functions.Worker.AuthorizationLevel;

namespace Nhs.Appointments.Api.Functions;

public class GetAuthTokenFunction(IHttpClientFactory httpClientFactory, IAuditWriteService auditWriteService, IOptions<AuthOptions> authOptions)
{
    private readonly AuthOptions _authOptions = authOptions.Value;

    [Function("GetAuthTokenFunction")]
    [AllowAnonymous]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "token")]
        HttpRequest req)
    {
        var code = await req.ReadAsStringAsync();
        var formValues = new Dictionary<string, string>
        {
            { "client_id", _authOptions.ClientId },
            { "code", code },
            { "redirect_uri", _authOptions.ReturnUri },
            { "scope", "openid profile email" },
            { "grant_type", "authorization_code" },
            { "code_verifier", _authOptions.ChallengePhrase }
        };

        if (string.IsNullOrEmpty(_authOptions.ClientSecret) == false)
        {
            formValues.Add("client_secret", _authOptions.ClientSecret);
        }

        var form = new FormUrlEncodedContent(formValues);
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync($"{_authOptions.TokenUri}", form);
        var rawResponse = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(rawResponse);

            _ = Task.Run(() => RecordAuditLogin(tokenResponse.IdToken));
            
            return new OkObjectResult(new { token = tokenResponse.IdToken });
        }

        throw new InvalidOperationException(
            $"Failed to retrieve token from identity provide\r\nReceived status code {response.StatusCode}\r\n{rawResponse}");
    }
    
    private async Task RecordAuditLogin(string idToken)
    {
        var token = new JwtSecurityToken(idToken);
        await auditWriteService.RecordAuth(token.Id, DateTime.UtcNow, token.Subject, AuditAuthActionType.Login);
    }
}
