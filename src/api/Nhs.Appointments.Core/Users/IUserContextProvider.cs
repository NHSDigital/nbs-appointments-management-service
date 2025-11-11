using System.Security.Claims;

namespace Nhs.Appointments.Core.Users;

public interface IUserContextProvider
{
    ClaimsPrincipal UserPrincipal { get; }
}

