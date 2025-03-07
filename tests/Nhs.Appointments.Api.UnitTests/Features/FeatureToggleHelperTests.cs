using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Moq;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.UnitTests;

namespace Nhs.Appointments.Api.Tests.Features;

public class FeatureToggleHelperTests
{
    private readonly Mock<IFeatureManager> _featureManager = new();
    private readonly Mock<FunctionContext> _functionContext = new();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsFeatureEnabled_WhenUserEnabledForFlag(bool isEnabled)
    {
        var testUser = "testUser";
        var testFlag = "testFlag";

        _featureManager
            .Setup(x => x.IsEnabledAsync(testFlag,
                It.Is<TargetingContext>(y => y.Groups == null && y.UserId == testUser))).ReturnsAsync(isEnabled);

        var sut = new FeatureToggleHelper(_featureManager.Object);
        var result = await sut.IsFeatureEnabled(testFlag, testUser, null);
        result.Should().Be(isEnabled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsFeatureEnabled_WhenSiteEnabledForFlag(bool isEnabled)
    {
        var testSite = "testSite";
        var testFlag = "testFlag";

        var array = new List<string> { $"Site:{testSite}" };

        _featureManager
            .Setup(x => x.IsEnabledAsync(testFlag,
                It.Is<TargetingContext>(y => y.Groups.SequenceEqual(array) && y.UserId == null)))
            .ReturnsAsync(isEnabled);

        var sut = new FeatureToggleHelper(_featureManager.Object);
        var result = await sut.IsFeatureEnabled(testFlag, "", [testSite]);
        result.Should().Be(isEnabled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsFeatureEnabledForFunction_FunctionWithSiteFromQueryStringInspector(bool isEnabled)
    {
        var testUser = "testUser";
        var testSite = "testSite";
        var testFlag = "testFlag";

        var testHttpRequestData = new TestHttpRequestData(_functionContext.Object);
        testHttpRequestData.Query.Add("site", testSite);

        var array = new List<string> { $"Site:{testSite}" };

        var principal = UserDataGenerator.CreateUserPrincipal(testUser);

        var mockHttpRequestDataFeature = new Mock<IHttpRequestDataFeature>();
        mockHttpRequestDataFeature.Setup(x => x.GetHttpRequestDataAsync(It.IsAny<FunctionContext>()))
            .ReturnsAsync(testHttpRequestData);
        var mockFeatures = new Mock<IInvocationFeatures>();
        mockFeatures.Setup(x => x.Get<IHttpRequestDataFeature>()).Returns(mockHttpRequestDataFeature.Object);

        _functionContext.Setup(x => x.Features).Returns(mockFeatures.Object);
        _featureManager
            .Setup(x => x.IsEnabledAsync(testFlag,
                It.Is<TargetingContext>(y => y.Groups.SequenceEqual(array) && y.UserId == testUser)))
            .ReturnsAsync(isEnabled);

        var sut = new FeatureToggleHelper(_featureManager.Object);
        var result = await sut.IsFeatureEnabledForFunction(testFlag, _functionContext.Object, principal,
            new SiteFromQueryStringInspector());
        result.Should().Be(isEnabled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsFeatureEnabledForFunction_FunctionWithMultiSiteBodyRequestInspector(bool isEnabled)
    {
        var testUser = "testUser";
        var testFlag = "testFlag";

        var testHttpRequestData = new TestHttpRequestData(_functionContext.Object);
        testHttpRequestData.SetBody("{\"sites\": [\"1234\", \"5678\"]}");

        var array = new List<string> { "Site:1234", "Site:5678" };

        var principal = UserDataGenerator.CreateUserPrincipal(testUser);

        var mockHttpRequestDataFeature = new Mock<IHttpRequestDataFeature>();
        mockHttpRequestDataFeature.Setup(x => x.GetHttpRequestDataAsync(It.IsAny<FunctionContext>()))
            .ReturnsAsync(testHttpRequestData);
        var mockFeatures = new Mock<IInvocationFeatures>();
        mockFeatures.Setup(x => x.Get<IHttpRequestDataFeature>()).Returns(mockHttpRequestDataFeature.Object);

        _functionContext.Setup(x => x.Features).Returns(mockFeatures.Object);
        _featureManager
            .Setup(x => x.IsEnabledAsync(testFlag,
                It.Is<TargetingContext>(y => y.Groups.SequenceEqual(array) && y.UserId == testUser)))
            .ReturnsAsync(isEnabled);

        var sut = new FeatureToggleHelper(_featureManager.Object);
        var result = await sut.IsFeatureEnabledForFunction(testFlag, _functionContext.Object, principal,
            new MultiSiteBodyRequestInspector());
        result.Should().Be(isEnabled);
    }
}
