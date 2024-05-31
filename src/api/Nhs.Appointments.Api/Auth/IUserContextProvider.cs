using System.Security.Claims;

namespace Nhs.Appointments.Api.Auth;

public interface IUserContextProvider
{
    ClaimsPrincipal UserPrincipal { get; }
}

