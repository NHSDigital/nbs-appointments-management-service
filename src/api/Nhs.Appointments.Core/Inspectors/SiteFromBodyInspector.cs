using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Core.Inspectors;

public class SiteFromBodyInspector : IRequestInspector
{
    public async Task<IEnumerable<string>> GetSiteIds(HttpRequestData httpRequest)
    {
        var body = await httpRequest.ReadBodyAsStringAndLeaveIntactAsync();

        if (string.IsNullOrEmpty(body) == false)
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(body);
                if (jsonDocument.RootElement.TryGetProperty("site", out var siteIdValue))
                {
                    return [siteIdValue.ToString()];
                }
            }
            catch (JsonException)
            {

            }
        }

        return Enumerable.Empty<string>();
    }
}
