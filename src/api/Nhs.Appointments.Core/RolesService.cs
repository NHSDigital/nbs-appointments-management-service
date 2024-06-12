namespace Nhs.Appointments.Core;

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
