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
        var lastRun = await Store.GetLastRun();

        var state = ResolveState(triggeredTime, lastRun);
        
        if (!state.HasDaysToRun)
        {
            return;
        }
        
        var startDate = state.DateRanTo?.AddDays(1) ?? state.From;
        var endDate = startDate.AddDays(Options.Value.DaysChunkSize);
        endDate = endDate > state.To ? state.To : endDate;
        
        var sites = (await SiteService.GetAllSites()).Select(x => x.Id).ToArray();

        await SendAggregationMessages(sites, startDate, endDate);

        await Store.SetLastRun(triggeredTime, state.From, state.To, endDate);
    }

    private AggregationState ResolveState(DateTimeOffset now, Aggregation aggregation)
    {
        if (aggregation is null) // First Run
        {
            return new AggregationState(Options.Value.FirstRunDate, DateTimeToDate(now.AddDays(Options.Value.DaysForward)), null);
        }

        var hasDaysToRun = aggregation.LastRanToDateOnly < aggregation.ToDateOnly;
        
        if (!hasDaysToRun) // Finished
        {
            if (DateTimeToDate(now) > DateTimeToDate(aggregation.LastTriggeredUtcDate)) // New Day
            {
                return new AggregationState(DateTimeToDate(aggregation.LastTriggeredUtcDate),
                    DateTimeToDate(now.AddDays(Options.Value.DaysForward)), null);
            }
        }
        
        return new AggregationState(aggregation.FromDateOnly, aggregation.ToDateOnly, aggregation.LastRanToDateOnly,
            hasDaysToRun);
    }
    
    private async Task SendAggregationMessages(string[] sites, DateOnly startDate, DateOnly endDate)
    {
        await bus.Send(sites.Select(site => new AggregateSiteSummaryEvent { Site = site, From = startDate, To = endDate }).ToArray());
    }
    
    private static DateOnly DateTimeToDate(DateTimeOffset dateTime)
    {
        return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
    }
}

public record AggregationState(DateOnly From, DateOnly To, DateOnly? DateRanTo, bool HasDaysToRun = true);
