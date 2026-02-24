using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.BlobAuditor;

public class AuditDataFilter(IOptions<DataFilterOptions> options, ILogger<DataProcessor> logger)
    : DataProcessor(options, logger), IDataFilter<JObject>
{
    public bool IsValidItem(JObject item)
    {
        var docType = item.Value<string>("docType");

        return docType switch
        {
            "site" => CanProcessSite(item.Value<string>("id")),
            _ => CanProcessSite(item.Value<string>("site"))
        };
    }
}
