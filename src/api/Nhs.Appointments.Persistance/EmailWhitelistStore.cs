using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class EmailWhitelistStore(ITypedDocumentCosmosStore<WhitelistedEmailDomainsDocument> cosmosStore, IFeatureToggleHelper featureToggleHelper) : IEmailWhitelistStore
{
    private const string WhitelistedEmailDomainsId = "whitelisted_email_domains";

    private readonly List<string> nhsEmailWhitelist = ["@nhs.net"];

    public async Task<IEnumerable<string>> GetWhitelistedEmails() =>
        await featureToggleHelper.IsFeatureEnabled(Flags.OktaEnabled)
            ? (await cosmosStore.GetDocument<WhitelistedEmailDomainsDocument>(WhitelistedEmailDomainsId)).Domains
            : nhsEmailWhitelist;
}
