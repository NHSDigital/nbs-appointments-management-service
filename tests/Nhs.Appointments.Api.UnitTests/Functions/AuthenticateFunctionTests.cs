using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;

namespace Nhs.Appointments.Api.Tests.Functions;

public class AuthenticateFunctionTests
{
    private readonly AuthenticateFunction _sut;
    private readonly Mock<IOptions<AuthOptions>> _options = new();

    public AuthenticateFunctionTests()
    {
        _options.Setup(x => x.Value).Returns(new AuthOptions 
        { 
            ProviderUri = "https://test.oauth.com", 
            AuthorizePath = "auth",
            ReturnUri = "http://localhost",
            ClientId = "123",
            ChallengePhrase = "123",
            JwksPath = "jwks",
            Issuer = "123",
            TokenPath = "token"
        });

        _sut = new AuthenticateFunction(_options.Object);
    }

    [Fact]
    public void Run_RedirectsToOidc_WhenInvoked()
    {            
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.QueryString = new QueryString("?redirect_uri=123");
        var result = _sut.Run(request);
        result.Should().BeOfType<RedirectResult>();
        (result as RedirectResult).Url.StartsWith("https://test.oauth.com/auth");
    }
}    
