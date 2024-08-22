namespace Nhs.Appointments.Api.Events
{
    public class UserRolesChanged
    {
        public string User { get; set; }
        public string[] Roles { get; set; }
    }
}
