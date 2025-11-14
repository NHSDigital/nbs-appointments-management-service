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
        var matchingSequences = new List<List<SessionInstance>>();

        // Slide a window of 'requiredCount' consecutive slots
        for (var i = 0; i <= filterdSlots.Count() - requiredCount; i++)
        {
            var sequence = filterdSlots.Skip(i).Take(requiredCount).ToList();

            if (AreConsecutive(sequence, requiredServices))
            {
                matchingSequences.Add(sequence);
            }
        }

        return matchingSequences
            .SelectMany(s => s)
            .Distinct()
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
    private static bool AreConsecutive(List<SessionInstance> sequence, HashSet<string> requiredServices)
    {
        var requiredCount = requiredServices.Count;

        for (var i = 0; i < sequence.Count - 1; i++)
        {
            if (sequence[i].Until != sequence[i + 1].From)
            {
                return false;
            }
        }

        if (sequence.Any(slot => !slot.Services.Any(s => requiredServices.Contains(s))))
        {
            return false;
        }

        var servicesCovered = sequence
            .SelectMany(s => s.Services)
            .Where(requiredServices.Contains)
            .Distinct()
            .Count();

        return servicesCovered == requiredCount;
    }

    /// <summary>
    /// Remove any slots which don't contain the required service type(s)
    /// Ensure capacity is > 0
    /// Also return a distinct list of required services for processing consecutive slots
    /// </summary>
    /// <param name="slots">All slots passed in from the BASS for a given date range</param>
    /// <param name="attendees">All attendees passed in from the API call</param>
    /// <returns></returns>
    private static (IEnumerable<SessionInstance> filteredSlots, HashSet<string> requiredServices) FilterSlotsByService(IEnumerable<SessionInstance> slots, IEnumerable<Attendee> attendees)
    {
        var requiredServices = attendees
            .SelectMany(a => a.Services)
            .Distinct()
            .ToHashSet();

        var filteredSlots = slots
            .Where(s => s.Services.Any(s => requiredServices.Contains(s)) && s.Capacity > 0)
            .OrderBy(s => s.From)
            .ToList();

        return (filteredSlots, requiredServices);
    }
}
