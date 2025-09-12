using System.Collections;

namespace Nhs.Appointments.Core;

public interface IBookingAvailabilityStateService
{
    Task<IEnumerable<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to);
    Task<AvailabilitySummary> GetWeekSummary(string site, DateOnly from);
    Task<AvailabilitySummary> GetDaySummary(string site, DateOnly day);
    Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from, DateTime to);
    Task<AvailabilityUpdateProposal> BuildRecalculations(string site, DateTime from, DateTime to, Session matcher, Session replacement);
}
