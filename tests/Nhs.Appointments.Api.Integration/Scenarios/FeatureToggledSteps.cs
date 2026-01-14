using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract class FeatureToggledSteps(string flag, bool enabled) : BaseFeatureSteps, IAsyncLifetime
{
    private string Flag { get; } = flag;
    private bool Enabled { get; } = enabled;

    public async Task InitializeAsync()
    {
        EnsureCollectionIsPresent();
        await SetLocalFeatureToggleOverride(Flag, Enabled ? "True" : "False");
    }

    public async Task DisposeAsync()
    {
        //disabled is default behaviour
        await SetLocalFeatureToggleOverride(Flag, "False");
    }

    protected void EnsureCollectionIsPresent()
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
