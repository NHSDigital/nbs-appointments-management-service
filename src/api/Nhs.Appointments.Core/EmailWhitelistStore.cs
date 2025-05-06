using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core;

public class EmailWhitelistStore(IFeatureToggleHelper featureToggleHelper) : IEmailWhitelistStore
{
    private readonly List<string> nhsEmailWhitelist = new() { "@nhs.net" };

    private readonly List<string> oktaEmailWhitelist =
        new()
        {
            "@nhs.net",
            "@gmail.com",
            "@yahoo.com",
            "@my-pharmacy.co.uk",
            "@okta.net"
        };

    public async Task<IEnumerable<string>> GetWhitelistedEmails() =>
        await featureToggleHelper.IsFeatureEnabled(Flags.OktaEnabled) ? oktaEmailWhitelist : nhsEmailWhitelist;
}
