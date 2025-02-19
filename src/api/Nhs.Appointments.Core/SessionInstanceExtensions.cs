namespace Nhs.Appointments.Core;
public static class SessionInstanceExtensions
{
    // Initially added to support joint bookings
    // This method will group consecutive availability by the consecutive input so that availability calculations will take into account consecutive bookings
    // Consecutive availability = availability that are one after another
    public static IEnumerable<SessionInstance> GroupByConsecutive(this IEnumerable<SessionInstance> slots, int consecutive)
    {
        // The logic should work with 1 but no need to do this computation
        if (consecutive <= 1)
        {
            return slots;
        }

        // Create consecutive time periods
        Func<SessionInstance, int, TimePeriod[]> generateConsecutivePeriods = (instance, concecutive) =>
                    Enumerable.Range(1, concecutive).Select(x => new TimePeriod(instance.From.Add(instance.Duration * x), instance.Duration)).ToArray();

        var consecutiveSlots = new List<SessionInstance>();

        foreach (var slot in slots)
        {
            var timePeriod = new TimePeriod(slot.From, slot.Duration * consecutive);
            var consecutivePeriods = generateConsecutivePeriods(slot, consecutive);
            var consecutiveCapacity = slots.Where(rs => consecutivePeriods.Any(cp => cp.From == rs.From && cp.Until == rs.Until));

            if (!FoundExpectedConsecutiveSlots(consecutiveCapacity, consecutivePeriods))
            {
                continue;
            }

            consecutiveSlots.Add(new SessionInstance(timePeriod)
            {
                Capacity = consecutiveCapacity.Min(x => x.Capacity),
                Services = consecutiveCapacity.SelectMany(x => x.Services).Distinct().ToArray()
            });
        }

        return consecutiveSlots;
    }

    private static bool FoundExpectedConsecutiveSlots(IEnumerable<SessionInstance> slots, TimePeriod[] periods) =>
        slots.Count() >= periods.Count();
}
