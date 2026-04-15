using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Core.Bookings;

public static class BookingSummaries
{
    public static List<DayAvailabilitySummary> InitialiseDaySummaries(
    DateTime from,
    DateTime to,
    List<LinkedSessionInstance> sessions)
    {
        var dayDate = DateOnly.FromDateTime(from.Date);

        List<DayAvailabilitySummary> daySummaries =
        [
            new(dayDate, InitialiseDailySessionSummaries(dayDate, sessions))
        ];

        while (dayDate < DateOnly.FromDateTime(to.Date))
        {
            dayDate = dayDate.AddDays(1);
            daySummaries.Add(new DayAvailabilitySummary(dayDate, InitialiseDailySessionSummaries(dayDate, sessions)));
        }

        return daySummaries;
    }

    public static AvailabilitySummary GenerateSummary(IEnumerable<Booking> bookings, List<DayAvailabilitySummary> daySummaries)
    {
        foreach (var daySummary in daySummaries)
        {
            var bookingsOnDay = bookings.Where(x => DateOnly.FromDateTime(x.From) == daySummary.Date).ToList();

            foreach (var booking in bookingsOnDay)
            {
                switch (booking.Status)
                {
                    case AppointmentStatus.Booked:
                        if (booking.AvailabilityStatus is AvailabilityStatus.Orphaned)
                        {
                            daySummary.TotalOrphanedAppointmentsByService[booking.Service] = daySummary.TotalOrphanedAppointmentsByService.GetValueOrDefault(booking.Service, 0) + 1;
                        }
                        break;
                    case AppointmentStatus.Cancelled:
                        daySummary.TotalCancelledAppointmentsByService[booking.Service] = daySummary.TotalCancelledAppointmentsByService.GetValueOrDefault(booking.Service, 0) + 1;
                        break;
                }
            }
        }

        return new AvailabilitySummary(daySummaries);
    }

    private static IEnumerable<SessionAvailabilitySummary> InitialiseDailySessionSummaries(DateOnly date,
        List<LinkedSessionInstance> sessionInstances)
    {
        return sessionInstances.Where(x => DateOnly.FromDateTime(x.From).Equals(date)).Select(x => new SessionAvailabilitySummary
        {
            Id = x.InternalSessionId!.Value,
            UkStartDatetime = x.From,
            UkEndDatetime = x.Until,
            MaximumCapacity = x.Capacity * x.ToSlots().Count(),
            Capacity = x.Capacity,
            SlotLength = x.SlotLength,
            TotalSupportedAppointmentsByService = x.Services.ToDictionary(key => key, _ => 0)
        }).ToList();
    }
}
