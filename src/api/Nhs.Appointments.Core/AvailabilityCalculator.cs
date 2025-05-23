namespace Nhs.Appointments.Core;

public interface IAvailabilityCalculator
{
    [Obsolete("This is only being kept around so that the code is the same before MultiServices are introduced." +
              "Once MultiServices are on and stay on, the feature flag and this code should be removed")]
    Task<IEnumerable<SessionInstance>>
        CalculateAvailability(string site, string service, DateOnly from, DateOnly until);
}

public class AvailabilityCalculator(
    IAvailabilityStore availabilityStore,
    IBookingsDocumentStore bookingDocumentStore,
    TimeProvider time) : IAvailabilityCalculator
{
    private readonly AppointmentStatus[] _liveStatuses = [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    [Obsolete("This is only being kept around so that the code is the same before MultiServices are introduced." +
              "Once MultiServices are on and stay on, the feature flag and this code should be removed")]
    public async Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from,
        DateOnly until)
    {
        var allSessions = await availabilityStore.GetSessions(site, from, until);
        var filteredSessions = service == "*"
            ? allSessions
            : allSessions.Where(s => s.Services.Contains(service));
        
        var bookings = await bookingDocumentStore.GetInDateRangeAsync(from.ToDateTime(new TimeOnly(0, 0)),
            until.ToDateTime(new TimeOnly(23, 59)), site);

        return GetAvailableSlots(filteredSessions, bookings);
    }

    private bool IsExpiredProvisional(Booking b) =>
        b.Status == AppointmentStatus.Provisional && b.Created < time.GetUtcNow().AddMinutes(-5);

    [Obsolete("This is only being kept around so that the code is the same before MultiServices are introduced." +
              "Once MultiServices are on and stay on, the feature flag and this code should be removed")]
    private IEnumerable<SessionInstance> GetAvailableSlots(IEnumerable<SessionInstance> sessions,
        IEnumerable<Booking> bookings)
    {
        var slots = sessions.SelectMany(session => session.ToSlots()).ToList();

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
