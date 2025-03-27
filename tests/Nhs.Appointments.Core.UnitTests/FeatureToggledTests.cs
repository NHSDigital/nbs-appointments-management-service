using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core.UnitTests;

public class FeatureToggledTests
{
    protected readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();

    protected void Toggle(string featureName, bool expectedValue) =>
        _featureToggleHelper
            .Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == featureName)))
            .ReturnsAsync(expectedValue);
}
