using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;


namespace Nhs.Appointments.Api.Auth;

public class SiteFromScopeInspector : IRequestInspector
{
    public async Task<IEnumerable<string>> GetSiteIds(HttpRequestData httpRequest)
    {
        var body = await httpRequest.ReadBodyAsStringAndLeaveIntactAsync();

        if (string.IsNullOrEmpty(body) == false)
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(body);
                if (jsonDocument.RootElement.TryGetProperty("scope", out var scopeValue))
                {
                    if (scopeValue.ToString().StartsWith("site:"))
                        return [scopeValue.ToString().Replace("site:", "")];
                }
            }
            catch (JsonException)
            {

            }
        }

        return Enumerable.Empty<string>();
    }
}
