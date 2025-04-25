using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core;

public class EmailWhitelistStore(IFeatureToggleHelper featureToggleHelper) : IEmailWhitelistStore
{
    private readonly List<string> nhsEmailWhitelist = new() { "@nhs.net" };
    private readonly List<string> oktaEmailWhitelist = new() { "@gmail.com", "@yahoo.com", "@my-pharmacy.co.uk" };

    public async Task<IEnumerable<string>> GetWhitelistedEmails() =>
        await featureToggleHelper.IsFeatureEnabled(Flags.OktaEnabled) ? oktaEmailWhitelist : nhsEmailWhitelist;
}
