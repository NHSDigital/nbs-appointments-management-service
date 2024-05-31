using System.Security.Claims;

namespace Nhs.Appointments.Api.Auth;

public class UserContextProvider : IUserContextProvider
{
    public ClaimsPrincipal UserPrincipal { get; set; }
}

