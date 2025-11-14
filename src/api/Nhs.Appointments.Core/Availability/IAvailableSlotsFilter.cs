namespace Nhs.Appointments.Core.Availability;
public interface IAvailableSlotsFilter
{
    IEnumerable<SessionInstance> FilterAvailableSlots(List<SessionInstance> slots, List<Attendee> attendees);
}
