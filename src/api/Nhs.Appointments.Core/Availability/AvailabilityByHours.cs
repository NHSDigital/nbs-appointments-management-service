namespace Nhs.Appointments.Core.Availability;
public class AvailabilityByHours
{
    public string Site {  get; set; }
    public List<Attendee> Attendees { get; set; }
    public DateOnly Date {  get; set; }
    public List<Hour> Hours { get; set; }

    public class Hour
    {
        public string From { get; set; }
        public string Until { get; set; }
    }
}
