using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core.Okta;
using Okta.Sdk.Api;
using Okta.Sdk.Model;
using OktaUser = Okta.Sdk.Model.User;

namespace Nhs.Appointments.Core.UnitTests.Okta;
public class OktaUserDirectoryTests
{
    private readonly Mock<ILogger<OktaUserDirectory>> _loggerMock = new();
    private readonly Mock<IUserApi> _userApiMock = new();
    private readonly OktaUserDirectory _sut;

    public OktaUserDirectoryTests()
    {
        _sut = new OktaUserDirectory(_userApiMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("ACTIVE", OktaUserStatus.Active)]
    [InlineData("SUSPENDED", OktaUserStatus.Active)]
    [InlineData("RECOVERY", OktaUserStatus.Active)]
    [InlineData("PASSWORD_EXPIRED", OktaUserStatus.Active)]
    [InlineData("LOCKED_OUT", OktaUserStatus.Active)]
    [InlineData("PROVISIONED", OktaUserStatus.Provisioned)]
    [InlineData("STAGED", OktaUserStatus.Deactivated)]
    [InlineData("DEPROVISIONED", OktaUserStatus.Deactivated)]
    [InlineData("TestDefault", OktaUserStatus.Unknown)]
    public async Task GetUserAsync_UsesStatus_MappsCorrectly(string oktaStatus, OktaUserStatus expectedMyaStatus)
    {
        var user = "user";
        var oktaUser = new OktaUser();
        oktaUser.Status = new UserStatus(oktaStatus);
        _userApiMock.Setup(x => x.GetUserAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(oktaUser);

        var result = await _sut.GetUserAsync(user);

        result.Status.Should().Be(expectedMyaStatus);
    }
}
