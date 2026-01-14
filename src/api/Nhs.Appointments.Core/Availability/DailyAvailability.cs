namespace Nhs.Appointments.Core.Availability
{
    public class DailyAvailability
    {
        public DateOnly Date { get; set; }
        public Session[] Sessions { get; set; }
        public string LastUpdatedBy = null;
    }
}
