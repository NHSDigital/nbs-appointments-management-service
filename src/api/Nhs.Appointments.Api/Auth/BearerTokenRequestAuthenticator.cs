using System;
using System.Linq;
using System.Security.Authentication;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using DnsClient.Internal;
using MassTransit.Serialization;
using Microsoft.Extensions.Logging;

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
        var issuerKeys = (await Task.WhenAll(_options.Providers.Select(async authOptions =>
            (authOptions.Issuer, await _jwksRetriever.GetKeys(authOptions.JwksUri))))).ToList();

        var allowedAudiences = _options.Providers.Select(authOptions => authOptions.ClientId).ToList();

        var tokenValidationParams = new TokenValidationParameters
        {
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                // resolve keys for a given issuer
                return issuerKeys.SingleOrDefault(ik => ik.Issuer == securityToken.Issuer).Item2;
            },
            AudienceValidator = (audiences, token, parameters) =>
            {
                return audiences.Any(a => allowedAudiences.Contains(a));
            },
            IssuerValidator = (issuer, token, parameters) =>
            {
                return issuerKeys.Any(ik => ik.Issuer == issuer)
                    ? issuer
                    : throw new AuthenticationException("Invalid Issuer");
            },
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
        
        try
        {
            return _validator.ValidateToken(encodedToken, tokenValidationParams, out var token);
        }
        catch (SecurityTokenValidationException)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
