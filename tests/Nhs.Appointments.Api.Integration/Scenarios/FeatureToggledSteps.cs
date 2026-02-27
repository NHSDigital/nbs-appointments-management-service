using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Scenarios;

/// <summary>
/// Use when only need a single feature flag
/// </summary>
public abstract class SingleFeatureToggledSteps(string flag, bool enabled)
    : FeatureToggledSteps([new FlagState(flag, enabled)]);

/// <summary>
/// Use directly when need multiple feature flag controls
/// </summary>
public abstract class MultipleFeatureToggledSteps(FlagState[] flagStates) : FeatureToggledSteps(flagStates);

public abstract class FeatureToggledSteps(FlagState[] flagStates) : AuditFeatureSteps, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        EnsureCollectionIsValid();
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

    private void EnsureCollectionIsValid()
    {
        var collectionAttribute = GetType()
            .GetCustomAttributes(typeof(CollectionAttribute), true)
            .FirstOrDefault() as CollectionAttribute;

        if (collectionAttribute is null)
        {
            throw new Exception(
                "Any test which inherits from FeatureToggledSteps must belong to a collection to ensure parallelism safety.");
        }
        
        EnsureFlagsAreInTheCollection();
    }

    /// <summary>
    /// Make sure the collection used is permitted to control the defined flags for this test
    /// </summary>
    private void EnsureFlagsAreInTheCollection()
    {
        var collectionName = GetType()
            .GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType == typeof(CollectionAttribute))
            ?.ConstructorArguments.FirstOrDefault().Value as string;

        var collectionFlags = collectionName!.Split('_')[0].Split('|');

        foreach (var flag in flagStates.Select(x => x.Flag))
        {
            if (!collectionFlags.Contains(flag))
            {
                throw new Exception(
                    $"The collection defined for this test {collectionName} is not allowed to control flag {flag}.");
            }
        }
    }
}

public record FlagState(string Flag, bool Enabled);
