using System.Security.Claims;

namespace Nhs.Appointments.Core;

public class UserContextProvider : IUserContextProvider
{
    public ClaimsPrincipal UserPrincipal { get; set; }
}

