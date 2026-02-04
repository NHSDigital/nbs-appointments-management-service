using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.Aggregator;

public class AggregateSiteSummaryEventFeedEventMapper : IFeedEventMapper<JObject, AggregateSiteSummaryEvent>
{
    public IEnumerable<AggregateSiteSummaryEvent> MapToEvents(IEnumerable<JObject> feedItems)
    {
        return feedItems.Where(IsValidDocument)
            .Select(item => new AggregateSiteSummaryEvent(ResolveSite(item), ResolveDate(item)))
            .Distinct();
    }

    private static string ResolveSite(JObject item) => item.Value<string>("site")!;
    private static DateOnly ResolveDate(JObject item)
    {
        switch (item.Value<string>("docType"))
        {
            case "booking":
                var from = item.Value<DateTime>("from");
                return new DateOnly(from.Year, from.Month, from.Day);
            case "daily_availability":
                var date = item.Value<string>("date")!;
                return DateOnly.Parse(date);
            default:
                throw new InvalidOperationException($"Unknown document type: {item.Value<string>("docType")}");
        }
    }

    private static bool IsValidDocument(JObject item)
    {
        var docType = item.Value<string>("docType");

        return (docType == "daily_availability" && item.ContainsKey("date") && item.ContainsKey("site")) || 
               (docType == "booking" && item.Value<string>("status") != "Provisional" && item.ContainsKey("from") && item.ContainsKey("site"));
    }
}
