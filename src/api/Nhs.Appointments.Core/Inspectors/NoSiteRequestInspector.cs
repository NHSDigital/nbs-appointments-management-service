using Microsoft.Azure.Functions.Worker.Http;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Core.Inspectors;

public class NoSiteRequestInspector : IRequestInspector
{
    public Task<IEnumerable<string>> GetSiteIds(HttpRequestData _)
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }
}
