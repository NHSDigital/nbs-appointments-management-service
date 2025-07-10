namespace Nhs.Appointments.Core.Reports.SiteSummary;

public class SiteSummaryAggregator(IBookingAvailabilityStateService bookingAvailabilityStateService, IDailySiteSummaryStore store, TimeProvider timeProvider) : ISiteSummaryAggregator
{
    public async Task AggregateForSite(string site, DateOnly from, DateOnly to)
    {
        var iterator = from;
        while (iterator <= to)
        {
            await AggregateForSiteDay(site, iterator);
            iterator = iterator.AddDays(1);
        }
    }

    private async Task AggregateForSiteDay(string site, DateOnly day)
    {
        var generatedAt = timeProvider.GetUtcNow();

        var summary = await bookingAvailabilityStateService.GetDaySummary(site, day);
        
        if (summary.MaximumCapacity <= 0 && summary.Orphaned.All(x => x.Value <= 0) && summary.DaySummaries.All(x => x.CancelledAppointments <= 0))
        {
            return;
        }

        var clinicalServices = summary.DaySummaries.SelectMany(daySummary => daySummary.Sessions.SelectMany(session => session.Bookings.Select(bookings => bookings.Key))).Distinct().ToArray();
        
        await store.CreateDailySiteSummary(new DailySiteSummary()
        {
            Site = site,
            Date = day,
            Bookings = clinicalServices.ToDictionary(
                service => service, 
                service => summary.DaySummaries.Sum(daySummaries => daySummaries.Sessions.Sum(x => x.Bookings.GetValueOrDefault(service, 0)))),
            Cancelled = summary.DaySummaries.Sum(daySummaries => daySummaries.CancelledAppointments),
            Orphaned = summary.Orphaned,
            RemainingCapacity = clinicalServices.ToDictionary(
                service => service, 
                service => summary.DaySummaries.Sum(daySummaries => daySummaries.Sessions.Sum(x => x.Bookings.ContainsKey(service) ? x.RemainingCapacity : 0))),
            GeneratedAtUtc = generatedAt
        });
    }
}
