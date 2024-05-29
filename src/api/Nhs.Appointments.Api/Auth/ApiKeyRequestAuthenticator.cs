using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth;


public class ApiKeyRequestAuthenticator : IRequestAuthenticator
{
    private readonly ApiKeyOptions _options;

    public ApiKeyRequestAuthenticator(IOptions<ApiKeyOptions> options)
    {
        _options = options.Value;
    }

    public Task<ClaimsPrincipal> AuthenticateRequest(string apiKey)
    {
        if(_options.ValidKeys.Contains(apiKey))
        {
            var apiClaimant = new ClaimsPrincipal(new ApiConsumerIdentity());
            return Task.FromResult(apiClaimant);
        }

        return Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}

public class ApiConsumerIdentity : IIdentity
{
    public string AuthenticationType => "ApiKey";

    public bool IsAuthenticated => true;

    public string Name => "Api User";
}

public class ApiKeyOptions
{
    public string[] ValidKeys { get; set; }
}
