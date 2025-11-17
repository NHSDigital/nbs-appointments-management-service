using Nhs.Appointments.Core.Extensions;

namespace Nhs.Appointments.Core.Availability;

public class AvailableSlotsFilterRecursive : IAvailableSlotsFilter
{
    /// <summary>
    ///     This method will group consecutive slots based on a number of attendees' and their required services
    ///     It will filter out any slots that don't match the requested services before processing
    ///     If only a single attendee is passed in, just return all slots that have already been filtered by service type and
    ///     have a capacity > 0
    /// </summary>
    /// <param name="slots">Available slots returned from the BASS for a given date range</param>
    /// <param name="attendees">
    ///     A list of attendees containing an array of their requested services (currently restricted to 1
    ///     service 14/11/25)
    /// </param>
    /// <returns></returns>
    public IEnumerable<SessionInstance> FilterAvailableSlots(List<SessionInstance> slots, List<Attendee> attendees)
    {
        if (attendees is null || attendees.Count == 0)
        {
            return slots.OrderBy(s => s.From);
        }

        if (slots is null || slots.Count == 0)
        {
            return new List<SessionInstance>();
        }

        var requiredServices = attendees.SelectMany(attendee => attendee.Services).ToList();

        var slotsInAValidGroupBookingChain = new List<SessionInstance>();
        foreach (var slot in slots)
        {
            var longestChainStartingFromThisSlot = BuildGroupBookingChain(slot, slots, requiredServices);
            if (longestChainStartingFromThisSlot.Count == requiredServices.Count)
            {
                slotsInAValidGroupBookingChain.AddRange(longestChainStartingFromThisSlot);
            }
        }

        // Slots can be found in multiple chains, so remove duplicates
        var result = slotsInAValidGroupBookingChain
            .Distinct()
            .OrderBy(s => s.From);
        return result;
    }

    private List<SessionInstance> BuildGroupBookingChain(SessionInstance candidateSlot,
        List<SessionInstance> allSlots,
        List<string> servicesYetToFulfil)
    {
        var serviceMatchesInSlot = candidateSlot.Services.Intersect(servicesYetToFulfil).ToList();
        if (!serviceMatchesInSlot.Any() || candidateSlot.Capacity < 1)
        {
            // Slot doesn't offer the services we need, return false
            return new List<SessionInstance>();
        }

        if (servicesYetToFulfil.Count == 1)
        {
            // Slot can support our last service, no need to recurse further
            return new List<SessionInstance> { candidateSlot };
        }

        var neighboursOfSlot = allSlots
            .Where(otherSlot =>
                otherSlot.From == candidateSlot.Until
                || otherSlot.Until == candidateSlot.From)
            .ToList();

        var validPath = new List<SessionInstance> { candidateSlot };
        foreach (var serviceMatch in serviceMatchesInSlot)
        {
            var remainingServices = servicesYetToFulfil.RemoveFirstOccurrence(serviceMatch);

            foreach (var neighbouringSlot in neighboursOfSlot)
            {
                validPath.AddRange(BuildGroupBookingChain(neighbouringSlot, allSlots, remainingServices));
            }
        }

        return validPath.Distinct().ToList();
    }
}
