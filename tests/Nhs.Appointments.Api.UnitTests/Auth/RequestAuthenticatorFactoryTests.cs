using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Auth;

public class RequestAuthenticatorFactoryTests
{
    private readonly RequestAuthenticatorFactory _sut;
    private readonly Mock<IServiceProvider> _serviceProvider = new();

    public RequestAuthenticatorFactoryTests()
    {
        var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
        mockAuthOptions.Setup(x => x.Value).Returns(new AuthOptions());
        var mockValidator = new Mock<ISecurityTokenValidator>();
        var mockJwksRetriever = new Mock<IJwksRetriever>();
        var mockLogger = new Mock<ILogger<BearerTokenRequestAuthenticator>>();
        _serviceProvider.Setup(x => x.GetService(typeof(BearerTokenRequestAuthenticator))).Returns(new BearerTokenRequestAuthenticator(mockValidator.Object, mockJwksRetriever.Object, mockAuthOptions.Object, mockLogger.Object));
        _sut = new RequestAuthenticatorFactory(_serviceProvider.Object);
    }

    [Fact]
    public void CreateAuthenticator_ReturnCorrectType_BasedOnScheme()
    {
        var result = _sut.CreateAuthenticator("Bearer");
        result.Should().BeOfType(typeof(BearerTokenRequestAuthenticator));
    }

    [Fact]
    public void CreateAuthenticator_ThrowsNotSupportedException_IfSchemeIsNotValid() 
    { 
        Action create = () => _sut.CreateAuthenticator("Invalid");
        create.Should().Throw<NotSupportedException>();
    }

}
