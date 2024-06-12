namespace Nhs.Appointments.Core;

public class Role
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string[] Permissions { get; set; }
}