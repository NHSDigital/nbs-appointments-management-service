namespace Nhs.Appointments.Core.Availability;
public class AvailabilityBySlots
{
    public string Site { get; set; }
    public List<Attendee> Attendees { get; set; }
    public DateTime From { get; set; }
    public DateTime Until { get; set; }
    public List<Slot> Slots { get; set; }
    
    public class Slot
    {
        public string From { get; set; }
        public string Until { get; set; }
        public string[] Services { get; set; }
    }
}
