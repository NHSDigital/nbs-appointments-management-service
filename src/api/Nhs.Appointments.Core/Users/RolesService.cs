using Nhs.Appointments.Core.Bookings;

namespace Nhs.Appointments.Core.Users;

public class RolesService(IRolesStore store) : IRolesService
{
    public Task<IEnumerable<Role>> GetRoles()
    {
        return store.GetRoles();
    }
}

public interface IRolesService
{
    Task<IEnumerable<Role>> GetRoles();
}
