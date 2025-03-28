using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core.UnitTests;

public class FeatureToggledTests
{
    protected readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();

    protected FeatureToggledTests(Type testClassType)
    {
        var customAttributes = Attribute.GetCustomAttributes(testClassType);
        foreach (var attribute in customAttributes)
        {
            if (attribute is MockedFeatureToggleAttribute toggle)
            {
                Toggle(toggle.Name, toggle.Value);
            }
        }
    }

    protected void Toggle(string featureName, bool expectedValue) =>
        _featureToggleHelper
            .Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == featureName)))
            .ReturnsAsync(expectedValue);
}
