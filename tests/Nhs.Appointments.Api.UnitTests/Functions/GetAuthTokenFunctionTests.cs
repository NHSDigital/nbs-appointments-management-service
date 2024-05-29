using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetAuthTokenFunctionTests
{
    private readonly GetAuthTokenFunction _sut;
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<IOptions<AuthOptions>> _options = new();

    public GetAuthTokenFunctionTests()
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

        _sut = new GetAuthTokenFunction(_httpClientFactory.Object, _options.Object);
    }

    public void Test()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var response = _sut.RunAsync(request);
    }
}
