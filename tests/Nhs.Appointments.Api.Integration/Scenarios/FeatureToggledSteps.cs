using System.Threading.Tasks;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract class FeatureToggledSteps(string flag, bool enabled) : BaseFeatureSteps, IAsyncLifetime
{
    private string Flag { get; } = flag;
    private bool Enabled { get; } = enabled;

    public async Task InitializeAsync()
    {
        await base.SetLocalFeatureToggleOverride(Flag, Enabled ? "True" : "False");
    }

    public async Task DisposeAsync()
    {
        await base.ClearLocalFeatureToggleOverrides();
        await Dispose_Bookings_Availability();
    }
}
