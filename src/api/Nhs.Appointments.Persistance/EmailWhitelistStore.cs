using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class EmailWhitelistStore(
    ITypedDocumentCosmosStore<WhitelistedEmailDomainsDocument> cosmosStore,
    IMetricsRecorder metricsRecorder) : IEmailWhitelistStore
{
    private const string WhitelistedEmailDomainsId = "whitelisted_email_domains";

    public async Task<IEnumerable<string>> GetWhitelistedEmails()
    {
        using (metricsRecorder.BeginScope(MetricScopes.EmailWhiteList.Get))
        {
            return (await cosmosStore.GetDocument<WhitelistedEmailDomainsDocument>(WhitelistedEmailDomainsId)).Domains;
        }
    }
}
