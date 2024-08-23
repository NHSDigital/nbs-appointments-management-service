using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Nhs.Appointments.Api.Auth;
using System.Security.Claims;

namespace Nhs.Appointments.Api.Tests.Auth;

public class BearerTokenRequestAuthenticatorTests
{
    private readonly BearerTokenRequestAuthenticator _sut;
    private readonly Mock<ISecurityTokenValidator> _validator = new();
    private readonly Mock<IJwksRetriever> _jwksRetriever = new();
    private readonly Mock<IOptions<AuthOptions>> _options = new();

    public BearerTokenRequestAuthenticatorTests()
    {
        _options.Setup(x => x.Value).Returns(new AuthOptions { JwksUri = "https://test.oauth.com/jwks" });
        _sut = new BearerTokenRequestAuthenticator(_validator.Object, _jwksRetriever.Object, _options.Object);
    }

    [Fact]
    public async Task AuthenticateRequest_ReturnsTrue_WhenTokenIsValid()
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, "username"),
            new Claim(ClaimTypes.NameIdentifier, "userId"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        SecurityToken token = null;
        _validator.Setup(x => x.ValidateToken("123", It.IsAny<TokenValidationParameters>(), out token)).Returns(claimsPrincipal);
        var result = await _sut.AuthenticateRequest("123", null);
        result.Should().NotBeNull();
        result.Identity.IsAuthenticated.Should().BeTrue();        
    }

    [Fact]
    public async Task AuthenticateRequest_ReturnsFalse_WhenTokenIsNotValid()
    {
        SecurityToken token = null;
        _validator.Setup(x => x.ValidateToken("123", It.IsAny<TokenValidationParameters>(), out token)).Throws(new SecurityTokenValidationException());
        var result = await _sut.AuthenticateRequest("123", null);
        result.Should().NotBeNull();
        result.Identity.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateRequest_RetrievesJwks_BeforeAuthenticating()
    {        
        SecurityToken token = null;
        _validator.Setup(x => x.ValidateToken("123", It.IsAny<TokenValidationParameters>(), out token)).Throws(new SecurityTokenValidationException());
        var result = await _sut.AuthenticateRequest("123", null);
        result.Should().NotBeNull();
        result.Identity.IsAuthenticated.Should().BeFalse();

        _jwksRetriever.Verify(x => x.GetKeys("https://test.oauth.com/jwks"), Times.Once());
    }
}
