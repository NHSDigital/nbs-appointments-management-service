namespace Nhs.Appointments.Core
{
    public class DailyAvailability
    {
        public DateOnly Date { get; set; }
        public Session[] Sessions { get; set; }
    }
}
