namespace Nhs.Appointments.Core;

public interface IBookingAvailabilityStateService
{
    Task<List<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to);
    Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from, DateTime to);
}
