using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class EmailWhitelistStore(ITypedDocumentCosmosStore<WhitelistedEmailDomainsDocument> cosmosStore) : IEmailWhitelistStore
{
    private const string WhitelistedEmailDomainsId = "whitelisted_email_domains";

    public async Task<IEnumerable<string>> GetWhitelistedEmails() => (await cosmosStore.GetDocument<WhitelistedEmailDomainsDocument>(WhitelistedEmailDomainsId)).Domains;
}
