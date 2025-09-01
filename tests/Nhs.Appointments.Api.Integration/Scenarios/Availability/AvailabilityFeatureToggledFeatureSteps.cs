using System.Threading.Tasks;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;

public abstract class AvailabilityFeatureToggledFeatureSteps(string flag, bool enabled)
    : AvailabilityBaseFeatureSteps, IAsyncLifetime
{
    private string Flag { get; } = flag;
    private bool Enabled { get; } = enabled;

    public async Task InitializeAsync()
    {
        await SetLocalFeatureToggleOverride(Flag, Enabled ? "True" : "False");
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}
