using System.Collections.Generic;

namespace Nhs.Appointments.Core;

public class Role
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ICollection<string> Permissions { get; set; }
}