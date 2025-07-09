using System.Collections;

namespace Nhs.Appointments.Core;

public interface IBookingAvailabilityStateService
{
    Task<bool> HasAvailability(string service, string site, DateTime from, DateTime to);
    Task<IEnumerable<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to);
    Task<Summary> GetWeekSummary(string site, DateOnly from);
    Task<Summary> GetDaySummary(string site, DateOnly day);
    Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from, DateTime to);
}
