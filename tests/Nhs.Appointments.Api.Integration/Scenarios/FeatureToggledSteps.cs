using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Scenarios;

/// <summary>
/// Use directly when need multiple feature flag controls
/// </summary>
public abstract class FeatureToggledSteps(FlagState[] flagStates) : AuditFeatureSteps, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        EnsureCollectionIsPresent();
        var flagTasks = flagStates.Select(async flagState =>
            await SetLocalFeatureToggleOverride(flagState.Flag, flagState.Enabled ? "True" : "False")).ToArray();
        await Task.WhenAll(flagTasks);
    }

    public async Task DisposeAsync()
    {
        //disabled is default behaviour
        //only need to disable any flags that were enabled for this test
        var flagTasks = flagStates.Where(x => x.Enabled).Select(async flagState =>
            await SetLocalFeatureToggleOverride(flagState.Flag, "False")).ToArray();
        await Task.WhenAll(flagTasks);
    }

    private void EnsureCollectionIsPresent()
    {
        var collectionAttribute = GetType()
            .GetCustomAttributes(typeof(CollectionAttribute), true)
            .FirstOrDefault() as CollectionAttribute;

        if (collectionAttribute is null)
        {
            throw new Exception(
                "Any test which inherits from FeatureToggledSteps must belong to a collection to ensure parallelism safety.");
        }
    }
}

/// <summary>
/// Use when only need a single feature flag
/// </summary>
public abstract class SingleFeatureToggledSteps(string flag, bool enabled)
    : FeatureToggledSteps([new FlagState(flag, enabled)]);

public record FlagState(string Flag, bool Enabled);
