namespace Nhs.Appointments.Core;

public interface IAvailabilityCalculator
{
    Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until);
}

public class AvailabilityCalculator(IAvailabilityStore availabilityStore, IBookingsDocumentStore bookingDocumentStore) : IAvailabilityCalculator
{
    public async Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until)
    {
        var allSessions = await availabilityStore.GetSessions(site, from, until);
        var sessionsForService = allSessions.Where(s => s.Services.Contains(service));   

        if (sessionsForService.Any())
        {
            var slots = new List<SessionInstance>();
            foreach (var session in sessionsForService)
            {
                slots.AddRange(session.Divide(TimeSpan.FromMinutes(session.SlotLength)).Select(sl => new SessionInstance(sl) { Services = session.Services, Capacity = session.Capacity }));
            }

            var bookings = await bookingDocumentStore.GetInDateRangeAsync(site, from.ToDateTime(new TimeOnly(0, 0)), until.ToDateTime(new TimeOnly(23, 59)));
            var isNotCancelled = (Booking b) => b.Outcome?.ToLower() != "cancelled";
            var liveBookings = bookings.Where(isNotCancelled);

            foreach (var booking in liveBookings)
            {
                var slot = slots.FirstOrDefault(s => s.Capacity > 0 && s.From == booking.From && s.Duration.TotalMinutes == booking.Duration && s.Services.Contains(booking.Service));
                if(slot != null)
                {
                    slot.Capacity--;
                }
            }

            return slots.Where(s => s.Capacity > 0);
        }
        return Enumerable.Empty<SessionInstance>();
    }
}
