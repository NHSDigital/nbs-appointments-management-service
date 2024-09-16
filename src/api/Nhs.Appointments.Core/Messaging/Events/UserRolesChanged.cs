namespace Nhs.Appointments.Core.Messaging.Events;

public class UserRolesChanged
{
    public string UserId { get; set; }
    public string SiteId { get; set; }
    public string[] AddedRoleIds { get; set; }
    public string[] RemovedRoleIds { get; set; }
}
