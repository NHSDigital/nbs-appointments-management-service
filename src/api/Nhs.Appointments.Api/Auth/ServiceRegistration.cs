﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Linq;

namespace Nhs.Appointments.Api.Auth
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddRequestInspectors(this IServiceCollection services)
        {
            var inspectorTypes = typeof(IRequestInspector).Assembly
            .GetTypes()
            .Where(t => typeof(IRequestInspector).IsAssignableFrom(t) && t.IsClass && t.IsAbstract == false);

            foreach (var type in inspectorTypes)
            {                                
                services.AddSingleton(type);
            }

            return services;
        }

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services) =>services.Configure<ApiKeyOptions>(opts =>
             {
                 opts.ValidKeys = Environment.GetEnvironmentVariable("API_KEYS")?.Split(",") ?? new string[0];
             })
            .Configure<AuthOptions>(opts =>
            {
                opts.ProviderUri = Environment.GetEnvironmentVariable("AuthProvider_ProviderUri");
                opts.Issuer = Environment.GetEnvironmentVariable("AuthProvider_Issuer");
                opts.AuthorizePath = Environment.GetEnvironmentVariable("AuthProvider_AuthorizePath");
                opts.TokenPath = Environment.GetEnvironmentVariable("AuthProvider_TokenPath");
                opts.JwksPath = Environment.GetEnvironmentVariable("AuthProvider_JwksPath");
                opts.ChallengePhrase = Environment.GetEnvironmentVariable("AuthProvider_ChallengePhrase");
                opts.ClientId = Environment.GetEnvironmentVariable("AuthProvider_ClientId");
                opts.ReturnUri = Environment.GetEnvironmentVariable("AuthProvider_ReturnUri");
            })
            .Configure<SignedRequestAuthenticator.Options>(opts =>
            {
                opts.RequestTimeTolerance = TimeSpan.FromMinutes(3);
            })
            .AddScoped<IUserContextProvider, UserContextProvider>()
            .AddSingleton<IRequestAuthenticatorFactory, RequestAuthenticatorFactory>()
            .AddRequestInspectors()
            .AddSingleton<SignedRequestAuthenticator>()
            .AddSingleton<BearerTokenRequestAuthenticator>()
            .AddSingleton<ApiKeyRequestAuthenticator>()
            .AddSingleton<IJwksRetriever, JwksRetriever>()
            .AddTransient<ISecurityTokenValidator, JwtSecurityTokenHandler>()
            .AddTransient<IRequestSigner, RequestSigner>()
            .AddMemoryCache();
        
    }
}
