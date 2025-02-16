namespace Nhs.Appointments.Core;
public static class SessionInstanceExtensions
{
    // Initialised added to support joint bookings
    // This method will group consecutive availability by the consecutive input so that availability calculations will take into account consecutive bookings
    // Consecutive availability = availability that are one after another
    public static IEnumerable<SessionInstance> GroupByConsecutive(this IEnumerable<SessionInstance> slots, int consecutive)
    {
        // The logic should work with 1 but no need to do this computation
        if (consecutive <= 1)
        {
            return slots;
        }

        var consecutiveSlots = new List<SessionInstance>();

        foreach (var slot in slots)
        {
            var timePeriod = new TimePeriod(slot.From, slot.Duration * consecutive);
            var relatedCapacity = slots.Where(rs => rs.From >= timePeriod.From && rs.Until <= timePeriod.Until && slot.Services.Equals(rs.Services));

            if (relatedCapacity.Count() <= 1) 
            {
                continue;
            }

            consecutiveSlots.Add(new SessionInstance(timePeriod)
            {
                Capacity = relatedCapacity.Min(x => x.Capacity),
                Services = relatedCapacity.SelectMany(x => x.Services).Distinct().ToArray()
            });
        }

        return consecutiveSlots;
    }
}
