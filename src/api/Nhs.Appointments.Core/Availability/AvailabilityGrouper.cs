using static Nhs.Appointments.Core.Availability.AvailabilityByHours;
using static Nhs.Appointments.Core.Availability.AvailabilityBySlots;

namespace Nhs.Appointments.Core.Availability;
public static  class AvailabilityGrouper
{
    public static DayEntry BuildDayAvailability(DateOnly date, IEnumerable<SessionInstance> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);

        var noon = 12;
        var amSlots = slots.Where(s => s.From.Hour < noon);
        var pmSlots = slots.Where(s => s.From.Hour >= noon);

        var blocks = new List<Block>();

        if (amSlots.Any())
        {
            var earliestAmStart = amSlots.Min(s => s.From.TimeOfDay);

            blocks.Add(new Block
            {
                From = earliestAmStart.ToString(@"hh\:mm"),
                Until = "12:00"
            });
        }

        if (pmSlots.Any())
        {
            var latestPmFinish = pmSlots.Max(s => s.Until.TimeOfDay);

            blocks.Add(new Block
            {
                From = "12:00",
                Until = latestPmFinish.ToString(@"hh\:mm")
            });
        }

        return new DayEntry
        {
            Date = date,
            Blocks = blocks
        };
    }

    public static AvailabilityByHours BuildHourAvailability(string site, DateOnly date, List<Attendee> attendees, IEnumerable<SessionInstance> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);

        return new AvailabilityByHours
        {
            Site = site,
            Attendees = attendees,
            Date = date,
            Hours = [.. slots
                .GroupBy(x => x.From.Hour)
                .OrderBy(g => g.Key)
                .Select(g => new Hour
                {
                    From = $"{g.Key:D2}:00",
                    Until = g.Key == 23 ? "00:00" : $"{g.Key + 1:D2}:00"
                })]
        };
    }

    public static AvailabilityBySlots BuildSlotsAvailability(string site, DateTime from, DateTime until, List<Attendee> attendees, IEnumerable<SessionInstance> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);

        return new AvailabilityBySlots
        {
            Site = site,
            Attendees = attendees,
            From = from,
            Until = until,
            Slots = [.. slots
                .Select(s => new {
                    s.From,
                    s.Duration,
                    NormalisedServices = string.Join(",", s.Services ?? [])
                })
                .GroupBy(x => (x.From, x.Duration, x.NormalisedServices))
                .OrderBy(g => g.Key)
                .Select(g => new Slot
                {
                    From = TimeOnly.FromDateTime(g.Key.From).ToString("HH:mm"),
                    Until = TimeOnly.FromDateTime(g.Key.From.Add(g.Key.Duration)).ToString("HH:mm"),
                    Services = g.Key.NormalisedServices.Split(",", StringSplitOptions.RemoveEmptyEntries)
                })]
        };
    }
}
