namespace Nhs.Appointments.Core.Availability;
public static  class AvailabilityGrouper
{
    public static DayEntry BuildDayAvailability(DateOnly date, IEnumerable<SessionInstance> slots)
    {
        var noon = 12;
        var amSlots = slots.Where(s => s.From.Hour < noon);
        var pmSlots = slots.Where(s => s.From.Hour >= noon);
        var hasSpillover = slots.Any(s => s.From.Hour < noon && s.Until.Hour > noon);

        var blocks = new List<Block>();

        if (amSlots.Any() || hasSpillover)
        {
            var earliestAmStart = amSlots.Any() ? amSlots.Min(s => s.From.TimeOfDay) : slots.Min(s => s.From.TimeOfDay);

            blocks.Add(new Block
            {
                From = earliestAmStart.ToString(@"hh\:mm"),
                Until = "12:00"
            });
        }

        if (pmSlots.Any() || hasSpillover)
        {
            var latestPmFinish = pmSlots.Any() ? pmSlots.Max(s => s.Until.TimeOfDay) : slots.Max(s => s.Until.TimeOfDay);

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
}
