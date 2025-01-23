using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Core.Inspectors;

public class NoSiteRequestInspector : IRequestInspector
{
    public Task<IEnumerable<string>> GetSiteIds(HttpRequestData _)
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }
}
