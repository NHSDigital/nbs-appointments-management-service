namespace Nhs.Appointments.Core.Availability;
public class AvailableSlotsFilter : IAvailableSlotsFilter
{
    /// <summary>
    /// This method will group consecutive slots based on a number of attendees' and their required services
    /// It will filter out any slots that don't match the requested services before processing
    /// If only a single attendee is passed in, just return all slots that have already been filtered by service type and have a capacity > 0
    /// </summary>
    /// <param name="slots">Available slots returned from the BASS for a given date range</param>
    /// <param name="attendees">A list of attendees containing an array of their requested services (currently restricted to 1 service 14/11/25)</param>
    /// <returns></returns>
    public IEnumerable<SessionInstance> FilterAvailableSlots(List<SessionInstance> slots, List<Attendee> attendees)
    {
        if (attendees is null || attendees.Count == 0)
        {
            return slots;
        }

        if (slots is null || slots.Count == 0)
        {
            return [];
        }

        var (filterdSlots, requiredServices) = FilterSlotsByService(slots, attendees);

        if (attendees.Count == 1)
        {
            return filterdSlots;
        }

        var requiredCount = attendees.Count;

        var matching = FindConsecutivePeriods(filterdSlots, requiredCount, requiredServices);

        return matching
            .SelectMany(m => m)
            .OrderBy(s => s.From)
            .ToList();
    }

    /// <summary>
    /// This method ensure slots are consecutive so the end time of one slot must match the start time of the next
    /// It ensures the required services are consecutive in any order
    /// It ensures all required services are also included in the set being returned
    /// </summary>
    /// <param name="sequence">The next 'n' (attendee count) slots in the sequence</param>
    /// <param name="requiredServices">List of distinct requested services for all attendees</param>
    /// <returns></returns>
    private static bool AreConsecutive(List<SessionInstance> sequence, string[] requiredServices)
    {
        sequence = [.. sequence.OrderBy(s => s.From)];

        // Scenario can be found in MultipleAttendees_MultipleServices_DifferentSlotLengths_ReturnCorrectConsecutiveSlots test
        // The issue lies here with indexing on multiple services
        // There is a chain of 3.. but the FLU one is 3rd and it's matching RSV is 1st not 2nd so this method returns false.
        // We need to be able to loop through every permutation of the sequence
        for (var i = 0; i < sequence.Count - 1; i++)
        {
            if (sequence[i].Until != sequence[i + 1].From)
            {
                return false;
            }
        }

        var servicesInSequence = sequence
            .Where(s => s.Capacity > 0)
            .SelectMany(s => s.Services)
            .Distinct()
            .ToList();

        return requiredServices.All(servicesInSequence.Contains);
    }

    /// <summary>
    /// Remove any slots which don't contain the required service type(s)
    /// Ensure capacity is > 0
    /// Also return a distinct list of required services for processing consecutive slots
    /// </summary>
    /// <param name="slots">All slots passed in from the BASS for a given date range</param>
    /// <param name="attendees">All attendees passed in from the API call</param>
    /// <returns></returns>
    private static (List<SessionInstance> filteredSlots, string[] requiredServices) FilterSlotsByService(IEnumerable<SessionInstance> slots, IEnumerable<Attendee> attendees)
    {
        var requiredServices = attendees
            .SelectMany(a => a.Services)
            .Distinct()
            .ToArray();

        var filteredSlots = slots
            .Where(s => s.Services.Any(s => requiredServices.Contains(s)) && s.Capacity > 0)
            .OrderBy(s => s.From)
            .ToList();

        return (filteredSlots, requiredServices);
    }

    private static List<List<SessionInstance>> FindConsecutivePeriods(List<SessionInstance> slots, int requiredCount, string[] requiredServices)
    {
        var matching = new List<List<SessionInstance>>();

        var lookup = slots
            .GroupBy(s => s.From)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var startSlot in slots)
        {
            var sequence = new List<SessionInstance> { startSlot };
            var current = startSlot;

            while (lookup.TryGetValue(current.Until, out var nextSlots))
            {
                // Add all slots that begin when the current ends
                sequence.AddRange(nextSlots);

                // Move forward through time using the first next slot 
                current = nextSlots.First();

                if (sequence.Count >= requiredCount)
                {
                    break;
                }
            }

            if (sequence.Count >= requiredCount && AreConsecutive(sequence, requiredServices))
            {
                matching.Add(sequence);
            }
        }

        return matching;
    }
}
