using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Messaging;

namespace Nhs.Appointments.Core.Reports.SiteSummary;

public class SiteSummaryTrigger(
    IOptions<SiteSummaryOptions> options,
    TimeProvider timeProvider,
    IAggregationStore store,
    ISiteService siteService,
    IMessageBus bus)
    : ISitesSummaryTrigger
{
    private IOptions<SiteSummaryOptions> Options { get; } = options;
    private TimeProvider TimeProvider { get; } = timeProvider;
    private IAggregationStore Store { get; } = store;
    private ISiteService SiteService { get; } = siteService;

    public async Task Trigger()
    {
        var triggeredTime = TimeProvider.GetUtcNow();
        var lastRunDate = await Store.GetLastRunDate();
        var startDate = lastRunDate is null
            ? Options.Value.FirstRunDate
            : DateTimeToDate(lastRunDate.Value); 
        var endDate = DateTimeToDate(triggeredTime.AddDays(Options.Value.DaysForward));
        var chunks = SplitDateRange(startDate, endDate, 30).ToArray();
        var sites = await SiteService.GetAllSites();

        foreach (var site in sites)
        {
            await TriggerForSite(site.Id, chunks);
        }

        await Store.SetLastRunDate(triggeredTime);
        
    }

    private async Task TriggerForSite(string site, (DateOnly startDate, DateOnly endDate)[] chunks)
    {
        await bus.Send(chunks.Select(chunk => new AggregateSiteSummaryEvent { Site = site, From = chunk.startDate, To = chunk.endDate }).ToArray());
    }
    
    private static DateOnly DateTimeToDate(DateTimeOffset dateTime)
    {
        return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
    }
    
    private static IEnumerable<(DateOnly startDate, DateOnly endDate)> SplitDateRange(DateOnly start, DateOnly end, int dayChunkSize)
    {
        DateOnly chunkEnd;
        while ((chunkEnd = start.AddDays(dayChunkSize)) < end)
        {
            yield return (start, chunkEnd);
            start = chunkEnd.AddDays(1);
        }
        yield return (start, end);
    }
}
