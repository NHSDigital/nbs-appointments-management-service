using FluentAssertions;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Moq;
using Nhs.Appointments.Api.Features;

namespace Nhs.Appointments.Api.Tests.Features;

public class FeatureToggleHelperTests
{
    private static readonly string TestFlag = "testFlag";
    private readonly Mock<IFeatureManager> _featureManager = new();
    private readonly Mock<IConfigurationRefresher> _configRefresher = new();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsFeatureEnabled_WhenEnabledGloballyForFlag(bool isEnabled)
    {
        _featureManager.Setup(x => x.IsEnabledAsync(TestFlag)).ReturnsAsync(isEnabled);

        var sut = new FeatureToggleHelper(_featureManager.Object, _configRefresher.Object);
        var result = await sut.IsFeatureEnabled(TestFlag);
        result.Should().Be(isEnabled);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task IsFeatureEnabled_ArgumentExceptions(string flag)
    {
        _featureManager.Setup(x => x.IsEnabledAsync(TestFlag)).ReturnsAsync(true);

        var sut = new FeatureToggleHelper(_featureManager.Object, _configRefresher.Object);
        var featureEnabled = async () => { await sut.IsFeatureEnabled(flag); };
        await featureEnabled.Should().ThrowAsync<ArgumentException>().WithMessage("FeatureFlag cannot be null or empty.");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task IsFeatureEnabledForUser_FlagArgumentException(string flag)
    {
        _featureManager.Setup(x => x.IsEnabledAsync(TestFlag)).ReturnsAsync(true);

        var sut = new FeatureToggleHelper(_featureManager.Object, _configRefresher.Object);
        var featureEnabled = async () => { await sut.IsFeatureEnabledForUser(flag, "testUser"); };
        await featureEnabled.Should().ThrowAsync<ArgumentException>().WithMessage("FeatureFlag cannot be null or empty.");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task IsFeatureEnabledForUser_UserArgumentException(string user)
    {
        _featureManager.Setup(x => x.IsEnabledAsync(TestFlag)).ReturnsAsync(true);

        var sut = new FeatureToggleHelper(_featureManager.Object, _configRefresher.Object);
        var featureEnabled = async () => { await sut.IsFeatureEnabledForUser(TestFlag, user); };
        await featureEnabled.Should().ThrowAsync<ArgumentException>().WithMessage("UserId cannot be null or empty.");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task IsFeatureEnabledForSite_FlagArgumentException(string flag)
    {
        _featureManager.Setup(x => x.IsEnabledAsync(TestFlag)).ReturnsAsync(true);

        var sut = new FeatureToggleHelper(_featureManager.Object, _configRefresher.Object);
        var featureEnabled = async () => { await sut.IsFeatureEnabledForSite(flag, "testUser"); };
        await featureEnabled.Should().ThrowAsync<ArgumentException>().WithMessage("FeatureFlag cannot be null or empty.");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task IsFeatureEnabledForSite_SiteArgumentException(string site)
    {
        _featureManager.Setup(x => x.IsEnabledAsync(TestFlag)).ReturnsAsync(true);

        var sut = new FeatureToggleHelper(_featureManager.Object, _configRefresher.Object);
        var featureEnabled = async () => { await sut.IsFeatureEnabledForSite(TestFlag, site); };
        await featureEnabled.Should().ThrowAsync<ArgumentException>().WithMessage("SiteId cannot be null or empty.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsFeatureEnabledForUser_WhenUserEnabledForFlag(bool isEnabled)
    {
        var testUser = "testUser";

        _featureManager
            .Setup(x => x.IsEnabledAsync(TestFlag,
                It.Is<TargetingContext>(y => y.Groups == null && y.UserId == testUser))).ReturnsAsync(isEnabled);

        var sut = new FeatureToggleHelper(_featureManager.Object, _configRefresher.Object);
        var result = await sut.IsFeatureEnabledForUser(TestFlag, testUser);
        result.Should().Be(isEnabled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsFeatureEnabledForSite_WhenSiteEnabledForFlag(bool isEnabled)
    {
        var testSite = "testSite";

        var array = new List<string> { $"Site:{testSite}" };

        _featureManager
            .Setup(x => x.IsEnabledAsync(TestFlag,
                It.Is<TargetingContext>(y => y.Groups.SequenceEqual(array) && y.UserId == null)))
            .ReturnsAsync(isEnabled);

        var sut = new FeatureToggleHelper(_featureManager.Object, _configRefresher.Object);
        var result = await sut.IsFeatureEnabledForSite(TestFlag, testSite);
        result.Should().Be(isEnabled);
    }
}
