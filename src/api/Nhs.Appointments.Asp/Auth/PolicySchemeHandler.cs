using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Asp.Auth;

public class PolicySchemeHandler : IAuthorizationPolicyProvider
{
    const string POLICY_PREFIX = "MYA";

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        
    }
    Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicy("MYAUTHPOLICY"));

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => throw new NotImplementedException();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => throw new NotImplementedException();
}
