using System.Collections;

namespace Nhs.Appointments.Core;

public interface IBookingAvailabilityStateService
{
    Task<IEnumerable<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to);
    Task<WeekSummary> GetWeekSummary(string site, DateOnly from);
    Task<IEnumerable<DaySummary>> GetSummaries(string site, DateTime from, DateTime to);
    Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from, DateTime to);
}
