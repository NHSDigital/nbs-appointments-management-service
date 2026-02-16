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
        var docType = item.Value<string>("docType");
        
        if (!CanProcessDocumentType(docType))
        {
            return false;
        }
        
        if (!CanProcessSite(item.Value<string>("site")))
        {
            return false;
        }

        //extra valid data checks
        switch (docType)
        {
            case "daily_availability":
                var validAvailabilityData = item.ContainsKey("date") && item.ContainsKey("site");
                if (!validAvailabilityData)
                {
                    logger.LogError("Invalid daily_availability document data");
                }
                return validAvailabilityData;
            case "booking":
                var validBookingData = item.ContainsKey("from") && item.ContainsKey("site");
                if (!validBookingData)
                {
                    logger.LogError("Invalid booking document data");
                }
                
                return validBookingData && item.Value<string>("status") != "Provisional";
            default:
                return true;
        }
    }
}
