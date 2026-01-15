using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Providers;
using Nhs.Appointments.Core.Users;
using System.Security.Claims;

namespace Nhs.Appointments.Api.Tests.Providers;

public class LastUpdatedByResolverTests
{
    private readonly LastUpdatedByResolver _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IOptions<ApplicationOptions>> _config = new();
    private readonly ApplicationOptions _options = new() { ApplicationName = "TestApp" };

    public LastUpdatedByResolverTests()
    {
        _config.Setup(x => x.Value).Returns(_options);

        _sut = new LastUpdatedByResolver(_userContextProvider.Object, _config.Object);
    }

    [Fact]
    public void GetLastUpdatedBy_UserFoundInContext_UserEmailReturned()
    {
        // Arrange
        var userId = "user@nhs.net";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _userContextProvider.Setup(x => x.UserPrincipal).Returns(principal);

        // Act
        var result = _sut.GetLastUpdatedBy();

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetLastUpdatedBy_UserNotFound_ApplicationNameReturned()
    {
        // Arrange
        _userContextProvider.Setup(x => x.UserPrincipal).Returns((ClaimsPrincipal)null);

        // Act
        var result = _sut.GetLastUpdatedBy();

        // Assert
        Assert.Equal(_options.ApplicationName, result);
    }

    [Fact]
    public void GetLastUpdatedBy_ClaimMissing_ApplicationNameReturned()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(principal);

        // Act
        var result = _sut.GetLastUpdatedBy();

        // Assert
        Assert.Equal(_options.ApplicationName, result);
    }
}
