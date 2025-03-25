namespace Nhs.Appointments.Core;
public class GroupSessionByConsecutive : IGroupSessionsByConsecutive
{
    // Initially added to support joint bookings
    // This method will group consecutive availability by the consecutive input so that availability calculations will take into account consecutive bookings
    // Consecutive availability = availability that are one after another
    public IEnumerable<SessionInstance> GroupByConsecutive(IEnumerable<SessionInstance> slots, int consecutive)
    {
        // The logic should work with 1 but no need to do this computation
        if (consecutive <= 1)
        {
            return slots;
        }

        var consecutiveSlots = new List<SessionInstance>();

        foreach (var slot in slots)
        {
            var consecutivePeriods = GenerateConsecutivePeriods(slot, consecutive);
            var consecutiveCapacity = slots.Where(rs => consecutivePeriods.Any(cp => cp.From == rs.From && cp.Until == rs.Until));

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
