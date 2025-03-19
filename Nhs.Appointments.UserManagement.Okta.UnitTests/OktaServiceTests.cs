using FluentAssertions;

namespace Nhs.Appointments.UserManagement.Okta.UnitTests;

public class OktaServiceTests
{
    private readonly OktaService _sut;
    private readonly Mock<IOktaUserDirectory> _oktaUserDirectory = new();

    private readonly string userEmail = "test@okta.com";
    private readonly string firstName = "first";
    private readonly string lastName = "last";

    public OktaServiceTests()
    {
        _sut = new OktaService(_oktaUserDirectory.Object);
    }

    [Fact]
    public async Task CreateIfNotExists_CreateUser_UserCreated()
    {
        _oktaUserDirectory.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);
        _oktaUserDirectory.Setup(x => x.CreateUserAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await _sut.CreateIfNotExists(userEmail, firstName, lastName);

        Assert.Multiple(
            () => result.Success.Should().BeTrue(),
            () => result.FailureReason.Should().BeNullOrEmpty(),
            () => _oktaUserDirectory.Verify(x => x.CreateUserAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once),
            () => _oktaUserDirectory.Verify(x => x.ReactivateUserAsync(It.IsAny<string>()), Times.Never)
        );
    }

    [Fact]
    public async Task CreateIfNotExists_CreateUser_CreationFailed()
    {
        _oktaUserDirectory.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);
        _oktaUserDirectory.Setup(x => x.CreateUserAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        var result = await _sut.CreateIfNotExists(userEmail, firstName, lastName);

        Assert.Multiple(
            () => result.Success.Should().BeFalse(),
            () => result.FailureReason.Should().Be("User could not be created"),
            () => _oktaUserDirectory.Verify(x => x.CreateUserAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once),
            () => _oktaUserDirectory.Verify(x => x.ReactivateUserAsync(It.IsAny<string>()), Times.Never)
        );
    }

    [Fact]
    public async Task CreateIfNotExists_UserExists()
    {
        var oktaUserResponse = new OktaUserResponse { 
            Created = new DateTimeOffset(2025, 3, 15, 15, 44, 44, TimeSpan.Zero), 
            IsActive = true 
        };
        _oktaUserDirectory.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(oktaUserResponse);

        var result = await _sut.CreateIfNotExists(userEmail, firstName, lastName);

        Assert.Multiple(
            () => result.Success.Should().BeTrue(),
            () => result.FailureReason.Should().BeNullOrEmpty()
        );
    }

    [Fact]
    public async Task CreateIfNotExists_ReactivateUser()
    {
        var oktaUserResponse = new OktaUserResponse
        {
            Created = new DateTimeOffset(2025, 3, 15, 15, 44, 44, TimeSpan.Zero),
            IsProvisioned = true
        };
        _oktaUserDirectory.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(oktaUserResponse);
        _oktaUserDirectory.Setup(x => x.ReactivateUserAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await _sut.CreateIfNotExists(userEmail, firstName, lastName);

        Assert.Multiple(
            () => result.Success.Should().BeTrue(),
            () => result.FailureReason.Should().BeNullOrEmpty(),
            () => _oktaUserDirectory.Verify(x => x.CreateUserAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never),
            () => _oktaUserDirectory.Verify(x => x.ReactivateUserAsync(It.IsAny<string>()), Times.Once)
        );
    }
}
