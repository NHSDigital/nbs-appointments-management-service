using System.Security.Claims;

namespace Nhs.Appointments.Core.Users;

public class UserContextProvider : IUserContextProvider
{
    public ClaimsPrincipal UserPrincipal { get; set; }
}

