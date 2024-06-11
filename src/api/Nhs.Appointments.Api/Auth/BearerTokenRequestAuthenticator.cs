using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth;

public class BearerTokenRequestAuthenticator : IRequestAuthenticator
{
    private readonly ISecurityTokenValidator _validator;
    private readonly IJwksRetriever _jwksRetriever;
    private readonly AuthOptions _options;

    public BearerTokenRequestAuthenticator(ISecurityTokenValidator validator, IJwksRetriever jwksRetriever, IOptions<AuthOptions> options)
    {
        _validator = validator;
        _jwksRetriever = jwksRetriever;
        _options = options.Value;
    }

    public async Task<ClaimsPrincipal> AuthenticateRequest(string encodedToken, HttpRequestData _)
    {
        var tokenValidationParams = new TokenValidationParameters
        {
            IssuerSigningKeys = await _jwksRetriever.GetKeys($"{_options.ProviderUri}/{_options.JwksPath}"),
            ValidAudience = _options.ClientId,
            ValidateAudience = true,
            ValidIssuer = _options.Issuer,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    
        try
        {
            return _validator.ValidateToken(encodedToken, tokenValidationParams, out var token);
        }
        catch (SecurityTokenValidationException)
        {
            ClaimsIdentity unauthenticated = new ClaimsIdentity();
            return new ClaimsPrincipal(unauthenticated);
        }
    }    
}
