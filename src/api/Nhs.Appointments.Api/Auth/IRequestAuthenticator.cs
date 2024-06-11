using Microsoft.Azure.Functions.Worker.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth;

public interface IRequestAuthenticator
{
    Task<ClaimsPrincipal> AuthenticateRequest(string authenticationToken, HttpRequestData request);
}
