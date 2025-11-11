using Nhs.Appointments.Core.Bookings;

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
        
        if (summary.MaximumCapacity <= 0 && summary.TotalOrphanedAppointmentsByService.All(x => x.Value <= 0) && summary.DaySummaries.All(x => x.TotalCancelledAppointments <= 0))
        {
            await store.IfExistsDelete(site, day);
            return;
        }
        
        await store.CreateDailySiteSummary(new DailySiteSummary
        {
            Site = site,
            Date = day,
            MaximumCapacity = summary.MaximumCapacity,
            Bookings = summary.TotalSupportedAppointmentsByService,
            Orphaned = summary.TotalOrphanedAppointmentsByService,
            Cancelled = summary.TotalCancelledAppointments,
            RemainingCapacity = summary.TotalRemainingCapacityByService,
            GeneratedAtUtc = generatedAt
        });
    }
}
