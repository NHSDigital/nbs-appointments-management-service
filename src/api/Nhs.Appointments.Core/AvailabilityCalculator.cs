namespace Nhs.Appointments.Core;

public interface IAvailabilityCalculator
{
    Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until);
}

public class AvailabilityCalculator : IAvailabilityCalculator
{
    private readonly IScheduleService _scheduleService;
    private readonly IBookingsDocumentStore _bookingDocumentStore;

    public AvailabilityCalculator(IScheduleService scheduleService, IBookingsDocumentStore bookingsDocumentStore)
    {
        _scheduleService = scheduleService;
        _bookingDocumentStore = bookingsDocumentStore;
    }

    public async Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until)
    {
        var sessions = await _scheduleService.GetSessions(site, service, from, until);
        if (sessions.Any())
        {
            var slots = new List<SessionInstance>();
            foreach (var session in sessions)
            {
                slots.AddRange(session.Divide(TimeSpan.FromMinutes(session.SlotLength)).Select(sl => new SessionInstance(sl) { Services = session.Services, Capacity = session.Capacity }));
            }

            var bookings = await _bookingDocumentStore.GetInDateRangeAsync(site, from.ToDateTime(new TimeOnly(0, 0)), until.ToDateTime(new TimeOnly(23, 59)));
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
