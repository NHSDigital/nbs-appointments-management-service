using FluentAssertions;
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
        var mockApiKeyOptions = new Mock<IOptions<ApiKeyOptions>>();
        mockApiKeyOptions.Setup(x => x.Value).Returns(new ApiKeyOptions());

        _serviceProvider.Setup(x => x.GetService(typeof(BearerTokenRequestAuthenticator))).Returns(new BearerTokenRequestAuthenticator(mockValidator.Object, mockJwksRetriever.Object, mockAuthOptions.Object));
        _serviceProvider.Setup(x => x.GetService(typeof(ApiKeyRequestAuthenticator))).Returns(new ApiKeyRequestAuthenticator(mockApiKeyOptions.Object));
        _sut = new RequestAuthenticatorFactory(_serviceProvider.Object);
    }

    [Theory]
    [InlineData("Bearer", typeof(BearerTokenRequestAuthenticator))]
    [InlineData("ApiKey", typeof(ApiKeyRequestAuthenticator))]
    public void CreateAuthenticator_ReturnCorrectType_BasedOnScheme(string scheme, Type expectedType)
    {
        var result = _sut.CreateAuthenticator(scheme);
        result.Should().BeOfType(expectedType);
    }

    [Fact]
    public void CreateAuthenticator_ThrowsNotSupportedException_IfSchemeIsNotValid() 
    { 
        Action create = () => _sut.CreateAuthenticator("Invalid");
        create.Should().Throw<NotSupportedException>();
    }

}
