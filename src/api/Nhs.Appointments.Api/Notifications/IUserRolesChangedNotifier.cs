using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public interface IUserRolesChangedNotifier
{
    Task Notify(string user, string site, string[] rolesAdded, string[] rolesRemoved);
}
