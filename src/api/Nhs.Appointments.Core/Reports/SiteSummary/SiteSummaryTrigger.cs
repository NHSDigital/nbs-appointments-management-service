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
    private TimeProvider TimeProvider { get;  } = timeProvider;
    private IAggregationStore Store { get;  } = store;
    private ISiteService SiteService { get;  } = siteService;

    public async Task Trigger()
    {
        var lastRunDate = await Store.GetLastRunDate(Options.Value.ReportName);
        var startDate = lastRunDate is null
            ? (DateOnly?)null
            : DateTimeToDate(lastRunDate.Value); 
        var endDate = DateTimeToDate(TimeProvider.GetUtcNow().AddDays(Options.Value.DaysForward));
        var sites = await SiteService.GetAllSites();

        foreach (var site in sites)
        {
            await TriggerForSite(site.Id, startDate, endDate);
        }
    }

    private static DateOnly DateTimeToDate(DateTimeOffset dateTime)
    {
        return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
    }

    public async Task TriggerForSite(string site, DateOnly? startDate, DateOnly endDate)
    {
        await bus.Send(new AggregateSiteSummaryEvent() { Site = site, From = startDate, To = endDate });
    }
}
