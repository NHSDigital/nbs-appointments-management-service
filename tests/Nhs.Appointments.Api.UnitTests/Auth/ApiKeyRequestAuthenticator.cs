using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Auth;

public class ApiKeyRequestAuthenticatorTests
{
    private readonly ApiKeyRequestAuthenticator _sut;
    private readonly Mock<IOptions<ApiKeyOptions>> _options = new();

    public ApiKeyRequestAuthenticatorTests()
    {
        _options.Setup(x => x.Value).Returns(new ApiKeyOptions { ValidKeys = new[] { "valid_key" } });
        _sut = new ApiKeyRequestAuthenticator(_options.Object);
    }

    [Fact]
    public async Task AuthenticateRequest_ReturnsTrue_WhenApiKeyIsValid()
    {        
        var result = await _sut.AuthenticateRequest("valid_key", null);
        result.Should().NotBeNull();
        result.Identity.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticateRequest_ReturnsFalse_WhenApiKeyIsNotValid()
    {
        var result = await _sut.AuthenticateRequest("invalid_key", null);
        result.Should().NotBeNull();
        result.Identity.IsAuthenticated.Should().BeFalse();
    }
}
