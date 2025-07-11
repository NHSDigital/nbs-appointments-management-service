namespace Nhs.Appointments.Core;

public interface IBookingAvailabilityStateService
{
    Task<(bool hasSlot, bool shortCircuited)> HasAnyAvailableSlot(string service, string site, DateTime from, DateTime to);
    Task<IEnumerable<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to);
    Task<WeekSummary> GetWeekSummary(string site, DateOnly from);
    Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from, DateTime to);
}
