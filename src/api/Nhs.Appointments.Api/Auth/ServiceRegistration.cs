using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nhs.Appointments.Core;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace Nhs.Appointments.Api.Auth;

public static class ServiceRegistration
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
    {
        // Set up configuration
        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        var configuration = builder.Build();

        services.Configure<AuthOptions>(opts => configuration.GetSection("Auth").Bind(opts));
        services.Configure<SignedRequestAuthenticator.Options>(opts =>
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

        return services;
    }
}
