using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;


namespace Nhs.Appointments.Api.Auth;

public class SiteFromScopeInspector : IRequestInspector
{
    public async Task<string> GetSiteId(HttpRequestData httpRequest)
    {
        var body = await httpRequest.ReadBodyAsStringAndLeaveIntactAsync();

        if (string.IsNullOrEmpty(body) == false)
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(body);
                if (jsonDocument.RootElement.TryGetProperty("scope", out var scopeValue))
                {
                    return scopeValue.ToString().StartsWith("site:") ? scopeValue.ToString().Replace("site:", "") : string.Empty;
                }
            }
            catch (JsonException)
            {

            }
        }

        return string.Empty;
    }
}
