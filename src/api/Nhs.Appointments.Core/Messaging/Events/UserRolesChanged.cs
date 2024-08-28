namespace Nhs.Appointments.Core.Messaging.Events
{
    public class UserRolesChanged
    {
        public string User { get; set; }
        public string[] Added { get; set; }
        public string[] Removed { get; set; }
    }
}
