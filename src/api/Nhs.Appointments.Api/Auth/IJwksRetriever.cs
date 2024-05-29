using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth;

public interface IJwksRetriever
{
    Task<IEnumerable<SecurityKey>> GetKeys(string jwksUri);
}
