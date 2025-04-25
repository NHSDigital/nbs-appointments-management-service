using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Persistance;

public class EmailWhitelistStore(IFeatureToggleHelper featureToggleHelper) : IEmailWhitelistStore
{
    public async Task<IEnumerable<string>> GetWhitelistedEmails() =>
        await featureToggleHelper.IsFeatureEnabled(Flags.OktaEnabled)
            ? new List<string>()
            : ["@nhs.net"];
}
