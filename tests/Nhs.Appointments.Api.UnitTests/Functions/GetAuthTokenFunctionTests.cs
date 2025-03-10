using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Audit.Services;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetAuthTokenFunctionTests
{
    private readonly Mock<IAuditWriteService> _auditWriteService = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<ILogger<GetAuthTokenFunction>> _logger = new();
    private readonly Mock<IOptions<AuthOptions>> _options = new();
    private readonly GetAuthTokenFunction _sut;

    public GetAuthTokenFunctionTests()
    {
        _options.Setup(x => x.Value).Returns(new AuthOptions
        {
            Providers =
            [
                new AuthProviderOptions
                {
                    Name = "test-auth",
                    AuthorizeUri = "https://test.oauth.com/auth",
                    ReturnUri = "http://localhost",
                    ClientId = "123",
                    ChallengePhrase = "123",
                    JwksUri = "https://test.oauth.com/jwks",
                    Issuer = "123",
                    TokenUri = "https://test.oauth.com/token"
                }
            ]
        });

        _sut = new GetAuthTokenFunction(_httpClientFactory.Object, _auditWriteService.Object, _options.Object);
    }

    public void Test()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var response = _sut.RunAsync(request);
    }
}
