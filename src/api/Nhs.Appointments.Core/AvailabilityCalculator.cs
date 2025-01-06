namespace Nhs.Appointments.Core;

public interface IAvailabilityCalculator
{
    Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until);

    IEnumerable<SessionInstance> GetAvailableSlots(IEnumerable<SessionInstance> sessions,
        IEnumerable<Booking> bookings);
}

public class AvailabilityCalculator(IAvailabilityStore availabilityStore, IBookingsDocumentStore bookingDocumentStore, TimeProvider time) : IAvailabilityCalculator
{
    private readonly AppointmentStatus[] _liveStatuses = [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    private bool IsExpiredProvisional(Booking b) =>
        b.Status == AppointmentStatus.Provisional && b.Created < time.GetUtcNow().AddMinutes(-5);

    public async Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until)
    {
        var allSessions = await availabilityStore.GetSessions(site, from, until);
        var filteredSessions = service == "*"
            ? allSessions
            : allSessions.Where(s => s.Services.Contains(service));

        var bookings = await bookingDocumentStore.GetInDateRangeAsync(from.ToDateTime(new TimeOnly(0, 0)),
            until.ToDateTime(new TimeOnly(23, 59)), site);

        return GetAvailableSlots(filteredSessions, bookings);
    }

    public IEnumerable<SessionInstance> GetAvailableSlots(IEnumerable<SessionInstance> sessions,
        IEnumerable<Booking> bookings)
    {
        var slots = new List<SessionInstance>();
        foreach (var session in sessions)
        {
            slots.AddRange(session.Divide(TimeSpan.FromMinutes(session.SlotLength)).Select(sl =>
                new SessionInstance(sl) { Services = session.Services, Capacity = session.Capacity }));
        }

        if (slots.Count == 0)
        {
            return [];
        }

        var liveBookings = bookings
            .Where(b => _liveStatuses.Contains(b.Status))
            .Where(b => IsExpiredProvisional(b) == false);

        foreach (var booking in liveBookings)
        {
            var slot = slots.FirstOrDefault(s =>
                s.Capacity > 0 && s.From == booking.From && (int)s.Duration.TotalMinutes == booking.Duration &&
                s.Services.Contains(booking.Service));
            if (slot != null)
            {
                slot.Capacity--;
            }
        }

        return slots.Where(s => s.Capacity > 0);
    }
}
