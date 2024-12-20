﻿namespace Nhs.Appointments.Core;

public interface IAvailabilityCalculator
{
    Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until);
}

public class AvailabilityCalculator(IAvailabilityStore availabilityStore, IBookingsDocumentStore bookingDocumentStore, TimeProvider time) : IAvailabilityCalculator
{
    public async Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until)
    {
        var allSessions = await availabilityStore.GetSessions(site, from, until);
        var filteredSessions = service == "*"
            ? allSessions
            : allSessions.Where(s => s.Services.Contains(service));

        if (filteredSessions.Any())
        {
            var slots = new List<SessionInstance>();
            foreach (var session in filteredSessions)
            {
                slots.AddRange(session.Divide(TimeSpan.FromMinutes(session.SlotLength)).Select(sl => new SessionInstance(sl) { Services = session.Services, Capacity = session.Capacity }));
            }

            var bookings = await bookingDocumentStore.GetInDateRangeAsync(from.ToDateTime(new TimeOnly(0, 0)), until.ToDateTime(new TimeOnly(23, 59)), site);

            var liveStatuses = new[] { AppointmentStatus.Booked, AppointmentStatus.Provisional };
            var isExpiredProvisional = (Booking b) => b.Status == AppointmentStatus.Provisional && b.Created < time.GetUtcNow().AddMinutes(-5);
            var liveBookings = bookings
                .Where(b => liveStatuses.Contains(b.Status))
                .Where(b => isExpiredProvisional(b) == false);

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
