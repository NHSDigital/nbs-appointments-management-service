using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Api.Auth;

public class NoSiteRequestInspector : IRequestInspector
{
    public Task<IEnumerable<string>> GetSiteIds(HttpRequestData _)
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }
}
