using System.Threading.Tasks;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.FeatureFlags;

public abstract class MultipleFeatureFlagsSteps(FlagState[] flagStates) : MultipleFeatureToggledSteps(flagStates)
{
    [When("I do nothing")]
    [Then("I do nothing")]
    public Task AssertNothing()
    {
        return Task.CompletedTask;
    }
}

public abstract class SingleFeatureFlagsSteps(string flag, bool enabled) : SingleFeatureToggledSteps(flag, enabled)
{
    [When("I do nothing")]
    [Then("I do nothing")]
    public Task AssertNothing()
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// You can still use individual flags that belong to multiple collections in a single set way, but the test must still be attributed with the multiple collection
/// </summary>
[Collection(FeatureToggleCollectionNames.TestMultipleCollection)]
[FeatureFile("./Scenarios/FeatureFlags/MultipleFeatureCollection_SingleUsage.feature")]
public class MultipleFeatureCollectionSingleUsage() : SingleFeatureFlagsSteps(Flags.TestFeatureSitesEnabled, true);

/// <summary>
/// You can set any number of flags within the collection to specific states. Any flags unspecified will have disabled state.
/// </summary>
[Collection(FeatureToggleCollectionNames.TestMultipleCollection)]
[FeatureFile("./Scenarios/FeatureFlags/MultipleFeatureCollection_MultipleUsage.feature")]
public class MultipleFeatureCollectionMultipleUsage() : MultipleFeatureFlagsSteps([new FlagState(Flags.TestFeaturePercentageEnabled, true), new FlagState(Flags.TestFeatureUsersEnabled, true)]);
