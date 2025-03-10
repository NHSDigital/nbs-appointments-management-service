using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;

namespace Nhs.Appointments.Api.Tests.Functions;

public class AuthenticateCallbackFunctionTests
{
    private readonly Mock<IOptions<AuthOptions>> _options = new();

    [Fact]
    public void Test()
    {
        var context = new DefaultHttpContext();
        var defaultHttpRequest = context.Request;
        defaultHttpRequest.QueryString = new QueryString("?provider=test-auth&code=123");

        var options = new Mock<IOptions<AuthOptions>>();
        options.Setup(x => x.Value).Returns(new AuthOptions
        {
            Providers =
            [
                new AuthProviderOptions
                {
                    Name = "test-auth",
                    AuthorizeUri = "https://test.oauth.com/auth",
                    ReturnUri = "http://localhost",
                    ClientId = "client-123",
                    ChallengePhrase = "123",
                    JwksUri = "https://test.oauth.com/jwks",
                    Issuer = "123",
                    TokenUri = "https://test.oauth.com/token",
                    ClientCodeExchangeUri = "http://test.some.com"
                }
            ]
        });

        var sut = new AuthenticateCallbackFunction(options.Object);
        var result = sut.Run(defaultHttpRequest);
        result.Should().BeOfType<RedirectResult>();
        (result as RedirectResult).Url.Should().Be("http://test.some.com:80/?code=123");
    }
}
