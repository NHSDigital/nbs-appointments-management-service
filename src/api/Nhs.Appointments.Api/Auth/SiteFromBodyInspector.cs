﻿using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;


namespace Nhs.Appointments.Api.Auth;

public class SiteFromBodyInspector : IRequestInspector
{
    public async Task<string> GetSiteId(HttpRequestData httpRequest)
    {
        var body = await httpRequest.ReadBodyAsStringAndLeaveIntactAsync();

        if (string.IsNullOrEmpty(body) == false)
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(body);
                if (jsonDocument.RootElement.TryGetProperty("site", out var siteIdValue))
                {
                    return siteIdValue.ToString();                    
                }
            }
            catch (JsonException)
            {

            }
        }

        return string.Empty;
    }
}
