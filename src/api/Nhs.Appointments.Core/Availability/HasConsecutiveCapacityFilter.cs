using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core.Helpers;

namespace Nhs.Appointments.Core.Availability;

public class HasConsecutiveCapacityFilter: IHasConsecutiveCapacityFilter
{
    // Initially added to support joint bookings
    // This method will group consecutive availability by the consecutive input so that availability calculations will take into account consecutive bookings
    // Consecutive availability = availability that are one after another
    public async Task<HashSet<SessionInstance>> SessionHasConsecutiveSessions(HashSet<SessionInstance> slots, int consecutive)
    {
        // The logic should work with 1 but no need to do this computation
        if (consecutive <= 1)
        {
            return slots;
        }

        // There is an important assumption here that every slot in the slots argument passed into this function contains the same service.
        // This code is only called when querying for a slot of 1 specific service type
        var parallelSlots = slots
            .GroupBy(slot =>
                new { slot.From, slot.Duration, Services = string.Join(",", slot.Services.OrderBy(s => s)) })
            .Select(group => new SessionInstance(new TimePeriod(group.Key.From, group.Key.Duration))
            {
                Capacity = group.Sum(slot => slot.Capacity), Services = group.First().Services
            }).ToHashSet();

        var consecutiveSlots = new List<SessionInstance>();
        
        await Parallel.ForEachAsync(parallelSlots, async (slot, _) =>
        {
            consecutiveSlots.Add(await ResolveConsecutiveCapacity(slot, parallelSlots, consecutive));
        });
        
        return consecutiveSlots.ToHashSet();
    }

    private static Task<SessionInstance> ResolveConsecutiveCapacity(SessionInstance slot, HashSet<SessionInstance> slots,
        int consecutive)
    {
        var consecutivePeriods = GenerateConsecutivePeriods(slot, consecutive);

        var consecutiveCapacity = slots
            .Where(rs => consecutivePeriods.Contains((rs.From, rs.Until))).ToHashSet();

        var result = new SessionInstance(new TimePeriod(slot.From, slot.Duration))
        {
            Capacity =
                FoundExpectedConsecutiveSlots(consecutiveCapacity, consecutivePeriods)
                    ? consecutiveCapacity.Min(x => x.Capacity)
                    : 0,
            Services = consecutiveCapacity.SelectMany(x => x.Services).Distinct().ToArray()
        };

        return Task.FromResult(result);
    }

    private static bool FoundExpectedConsecutiveSlots(HashSet<SessionInstance> slots, HashSet<(DateTime From, DateTime Until)> periods) =>
        slots.Count >= periods.Count;

    private static HashSet<(DateTime From, DateTime Until)> GenerateConsecutivePeriods(SessionInstance instance, int consecutive)
    {
        var periods = new HashSet<(DateTime From, DateTime Until)>();
        var start = instance.From;
        for (var i = 0; i < consecutive; i++)
        {
            periods.Add((start.Add(instance.Duration * i), start.Add(instance.Duration * (i + 1))));
        }
        
        return periods;
    }
}
