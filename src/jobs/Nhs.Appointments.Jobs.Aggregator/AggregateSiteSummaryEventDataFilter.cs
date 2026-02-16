using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.Aggregator;

public class AggregateSiteSummaryEventDataFilter(IOptions<DataFilterOptions> options, ILogger<DataProcessor> logger)
    : DataProcessor(options, logger), IDataFilter<JObject>
{
    public bool IsValidItem(JObject item)
    {
        if (!CanProcessDocumentType(item.Value<string>("docType")))
        {
            return false;
        }
        
        if (!CanProcessSite(item.Value<string>("site")))
        {
            return false;
        }
    
        return (item.ContainsKey("date") && item.ContainsKey("site")) || 
               (item.Value<string>("status") != "Provisional" && item.ContainsKey("from") && item.ContainsKey("site"));
    }
}
