using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Linq;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Auth;

public static class ServiceRegistration
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services) =>services
        .Configure<AuthOptions>(opts =>
        {
            opts.Issuer = Environment.GetEnvironmentVariable("AuthProvider_Issuer");
            opts.AuthorizeUri = Environment.GetEnvironmentVariable("AuthProvider_AuthorizeUri");
            opts.TokenUri = Environment.GetEnvironmentVariable("AuthProvider_TokenUri");
            opts.JwksUri = Environment.GetEnvironmentVariable("AuthProvider_JwksUri");
            opts.ChallengePhrase = Environment.GetEnvironmentVariable("AuthProvider_ChallengePhrase");
            opts.ClientId = Environment.GetEnvironmentVariable("AuthProvider_ClientId");
            opts.ReturnUri = Environment.GetEnvironmentVariable("AuthProvider_ReturnUri");
            opts.ClientCodeExchangeUri = Environment.GetEnvironmentVariable("AuthProvider_ClientCodeExchangeUri");
            opts.ClientSecret = Environment.GetEnvironmentVariable("AuthProvider_ClientSecret");
        })
        .Configure<SignedRequestAuthenticator.Options>(opts =>
        {
            opts.RequestTimeTolerance = TimeSpan.FromMinutes(3);
        })
        .AddScoped<IUserContextProvider, UserContextProvider>()
        .AddSingleton<IRequestAuthenticatorFactory, RequestAuthenticatorFactory>()
        .AddSingleton<SignedRequestAuthenticator>()
        .AddSingleton<BearerTokenRequestAuthenticator>()
        .AddSingleton<IJwksRetriever, JwksRetriever>()
        .AddTransient<ISecurityTokenValidator, JwtSecurityTokenHandler>()
        .AddTransient<IRequestSigner, RequestSigner>()
        .AddMemoryCache();
}
