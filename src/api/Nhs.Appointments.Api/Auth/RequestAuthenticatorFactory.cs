using Microsoft.Extensions.DependencyInjection;
using System;

namespace Nhs.Appointments.Api.Auth;

public class RequestAuthenticatorFactory : IRequestAuthenticatorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RequestAuthenticatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IRequestAuthenticator CreateAuthenticator(string scheme) => scheme switch
    {
        "Bearer" => _serviceProvider.GetService<BearerTokenRequestAuthenticator>(),
        "ApiKey" => _serviceProvider.GetService<ApiKeyRequestAuthenticator>(),
        _ => throw new NotSupportedException()
    };
}    
