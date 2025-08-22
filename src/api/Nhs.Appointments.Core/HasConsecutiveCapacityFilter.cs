namespace Nhs.Appointments.Core;
public class HasConsecutiveCapacityFilter : IHasConsecutiveCapacityFilter
{
    // Initially added to support joint bookings
    // This method will group consecutive availability by the consecutive input so that availability calculations will take into account consecutive bookings
    // Consecutive availability = availability that are one after another
    public IEnumerable<SessionInstance> SessionHasConsecutiveSessions(IEnumerable<SessionInstance> slots, int consecutive)
    {
        // The logic should work with 1 but no need to do this computation
        if (consecutive <= 1)
        {
            return slots;
        }

        var consecutiveSlots = new List<SessionInstance>();

        // There is an important assumption here that every slot in the slots argument passed into this function contains the same service.
        // This code is only called when querying for a slot of 1 specific service type
        var parallelSlots = slots
            .GroupBy(slot =>
                new { slot.From, slot.Duration, Services = string.Join(",", slot.Services.OrderBy(s => s)) })
            .Select(group => new SessionInstance(new TimePeriod(group.Key.From, group.Key.Duration))
            {
                Capacity = group.Sum(slot => slot.Capacity), Services = group.First().Services
            });

        foreach (var slot in parallelSlots)
        {
            var consecutivePeriods = GenerateConsecutivePeriods(slot, consecutive);
            var consecutiveCapacity = parallelSlots.Where(rs =>
                consecutivePeriods.Any(cp => cp.From == rs.From && cp.Until == rs.Until));

            consecutiveSlots.Add(new SessionInstance(new TimePeriod(slot.From, slot.Duration))
            {
                Capacity = FoundExpectedConsecutiveSlots(consecutiveCapacity, consecutivePeriods) ? consecutiveCapacity.Min(x => x.Capacity) : 0,
                Services = consecutiveCapacity.SelectMany(x => x.Services).Distinct().ToArray()
            });
        }

        return consecutiveSlots;
    }

    private static bool FoundExpectedConsecutiveSlots(IEnumerable<SessionInstance> slots, TimePeriod[] periods) =>
        slots.Count() >= periods.Count();

    private static TimePeriod[] GenerateConsecutivePeriods(SessionInstance instance, int concecutive) =>
        Enumerable.Range(1, concecutive).Select(x => new TimePeriod(instance.From.Add(instance.Duration * (x - 1)), instance.Duration)).ToArray();
}
