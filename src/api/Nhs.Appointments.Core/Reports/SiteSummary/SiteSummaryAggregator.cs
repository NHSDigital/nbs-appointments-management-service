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
        var summary = await FetchSummary(bookingAvailabilityStateService, site, day);

        if (ShouldDeleteSummary(summary))
        {
            await store.IfExistsDelete(site, day);
            return;
        }

        var dailySummary = BuildDailySummary(site, day, summary);

        await store.CreateDailySiteSummary(dailySummary);
    }

    private static async Task<Summary> FetchSummary(IBookingAvailabilityStateService bookingAvailabilityStateService, string site, DateOnly day) =>
            await bookingAvailabilityStateService.GetDaySummary(site, day);
    private static bool ShouldDeleteSummary(Summary summary) => summary.MaximumCapacity <= 0 && summary.Orphaned.All(x => x.Value <= 0) && summary.DaySummaries.All(x => x.CancelledAppointments <= 0);

    private DailySiteSummary BuildDailySummary(string site, DateOnly day, Summary summary)
    {
        var generatedAt = timeProvider.GetUtcNow();

        var clinicalServices = DistinctClinicalServicesFromDaySummaries(summary);

        var dailySummary = new DailySiteSummary
        {
            Site = site,
            Date = day,
            Bookings = clinicalServices.ToDictionary(
                service => service,
                service => GetBookingsForService(summary, service)),
            Cancelled = summary.DaySummaries.Sum(daySummaries => daySummaries.CancelledAppointments),
            Orphaned = summary.Orphaned,
            RemainingCapacity = clinicalServices.ToDictionary(
                service => service,
                service => GetRemainingCapacityForService(summary, service)),
            GeneratedAtUtc = generatedAt,
            MaximumCapacity = summary.MaximumCapacity
        };

        return dailySummary;
    }

    private static int GetBookingsForService(Summary summary, string service) => 
        summary.DaySummaries.Sum(daySummaries => daySummaries.Sessions.Sum(x => x.Bookings.GetValueOrDefault(service, 0)));
    
    private static int GetRemainingCapacityForService(Summary summary, string service) => 
        summary.DaySummaries
            .SelectMany(ds => ds.Sessions)
            .Where(s => s.Bookings.ContainsKey(service))
            .Sum(s => s.RemainingCapacity);

    private string[] DistinctClinicalServicesFromDaySummaries(Summary summary) =>
        summary.DaySummaries.SelectMany(daySummary =>
                daySummary.Sessions.SelectMany(session => session.Bookings.Select(bookings => bookings.Key)))
            .Union(summary.Orphaned.Select(x => x.Key))
            .Distinct().ToArray();
}
