using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Core.Inspectors;

public class MultiSiteBodyRequestInspector : IRequestInspector
{
    public async Task<IEnumerable<string>> GetSiteIds(HttpRequestData httpRequest)
    {
        var body = await httpRequest.ReadBodyAsStringAndLeaveIntactAsync();

        if (string.IsNullOrEmpty(body) == false)
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(body);
                if (jsonDocument.RootElement.TryGetProperty("sites", out var sitesElement))
                {
                    return sitesElement.Deserialize<string[]>();
                }
            }
            catch (JsonException)
            {

            }
        }

        return Enumerable.Empty<string>();
    }
}
