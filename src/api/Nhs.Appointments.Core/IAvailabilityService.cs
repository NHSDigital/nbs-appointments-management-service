namespace Nhs.Appointments.Core;

public interface IAvailabilityService
{
    Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template, ApplyAvailabilityMode mode, string user);

    Task ApplySingleDateSessionAsync(DateOnly date, string site, Session[] sessions, ApplyAvailabilityMode mode,
        string user, Session sessionToEdit = null);
    Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEventsAsync(string site, DateOnly from);
    Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to);
    Task CancelSession(string site, DateOnly date, string from, string until, string[] services, int slotLength, int capacity);
    Task<AvailabilityState> GetAvailabilityState(string site, DateOnly from, DateOnly to, string serviceToQuery = null);
    Task RecalculateAppointmentStatuses(string site, DateOnly from, DateOnly to);
    Task<(bool Success, string Reference)> MakeBooking(Booking booking);
    Task<BookingCancellationResult> CancelBooking(string bookingReference, string site);
}
