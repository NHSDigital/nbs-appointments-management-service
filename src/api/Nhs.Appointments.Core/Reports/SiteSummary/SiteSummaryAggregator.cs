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
        
        if (summary.MaximumCapacity <= 0 && summary.Orphaned.All(x => x.Value <= 0) && summary.DaySummaries.All(x => x.CancelledAppointments.Sum(cancelled => cancelled.Value) <= 0))
        {
            await store.IfExistsDelete(site, day);
            return;
        }

        var clinicalServices = DistinctClinicalServicesFromDaySummaries(summary);

        var cancellationReasons = Enum.GetNames(typeof(CancellationReason)).ToList();
        
        await store.CreateDailySiteSummary(new DailySiteSummary
        {
            Site = site,
            Date = day,
            Bookings = clinicalServices.ToDictionary(
                service => service, 
                service => summary.DaySummaries.Sum(daySummaries => daySummaries.Sessions.Sum(x => x.Bookings.GetValueOrDefault(service, 0)))),
            Cancelled = cancellationReasons.ToDictionary(reason => reason, reason => summary.DaySummaries.Sum(daySummary => daySummary.CancelledAppointments.GetValueOrDefault(reason, 0))),
            Orphaned = summary.Orphaned,
            RemainingCapacity = clinicalServices.ToDictionary(
                service => service, 
                service => summary.DaySummaries.Sum(daySummaries => daySummaries.Sessions.Sum(x => x.Bookings.ContainsKey(service) ? x.RemainingCapacity : 0))),
            GeneratedAtUtc = generatedAt,
            MaximumCapacity = summary.MaximumCapacity
        });
    }

    private string[] DistinctClinicalServicesFromDaySummaries(Summary summary) =>
        summary.DaySummaries.SelectMany(daySummary =>
                daySummary.Sessions.SelectMany(session => session.Bookings.Select(bookings => bookings.Key)))
            .Union(summary.Orphaned.Select(x => x.Key))
            .Distinct().ToArray();
}
