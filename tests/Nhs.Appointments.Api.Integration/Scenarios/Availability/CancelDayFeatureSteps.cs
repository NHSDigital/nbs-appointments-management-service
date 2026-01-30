using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nhs.Appointments.Api.Integration.Data;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;

[FeatureFile("./Scenarios/Availability/CancelDay.feature")]
public class CancelDayFeatureSteps : BaseFeatureSteps
{
    private HttpResponseMessage Response { get; set; }

    [When("I cancel the day '(.+)'")]
    public async Task CancelDay(string dateString)
    {
        var date = NaturalLanguageDate.Parse(dateString);
        var site = GetSiteId();

        var payload = new
        {
            site,
            date
        };
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await GetHttpClientForTest().PostAsync("http://localhost:7071/api/day/cancel", content);
        Response.EnsureSuccessStatusCode();
    }
}
