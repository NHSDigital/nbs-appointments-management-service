using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CancelSession;

[FeatureFile("./Scenarios/CancelSession/CancelSession.feature")]
public class CancelSessionFeatureSteps : BaseFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private Core.Booking _actualResponse;

    [When("I cancel the following session")]
    public async Task CancelSession(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;
        var date = DateTime.ParseExact(ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"), "yyyy-MM-dd", null);

        object payload = new
        {
            site = GetSiteId(),
            date = DateOnly.FromDateTime(date),
            until = cells.ElementAt(2).Value,
            from = cells.ElementAt(1).Value,
            services = new string[] { cells.ElementAt(3).Value },
            slotLength = int.Parse(cells.ElementAt(4).Value),
            capacity = int.Parse(cells.ElementAt(5).Value)
        };

        _response = await Http.PostAsJsonAsync("http://localhost:7071/api/session/cancel", payload);
    }

    [When("I query for a booking with the reference number '(.+)'")]
    public async Task QueryBooking(string reference)
    {
        _response = await Http.GetAsync($"http://localhost:7071/api/booking/{reference}");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<Core.Booking>(await _response.Content.ReadAsStreamAsync());
    }

    [Then("the following booking is returned")]
    public void AssertBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        var expectedBooking = new Core.Booking
        {
            Reference = cells.ElementAt(4).Value,
            From = DateTime.ParseExact($"{ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {cells.ElementAt(1).Value}", "yyyy-MM-dd HH:mm", null),
            Duration = int.Parse(cells.ElementAt(2).Value),
            Service = cells.ElementAt(3).Value,
            Site = GetSiteId(),
        };

        _actualResponse.Reference.Should().Be(expectedBooking.Reference);
        _actualResponse.From.Should().Be(expectedBooking.From);
        _actualResponse.Duration.Should().Be(expectedBooking.Duration);
        _actualResponse.Service.Should().Be(expectedBooking.Service);
        _actualResponse.Site.Should().Be(expectedBooking.Site);
    }
}
