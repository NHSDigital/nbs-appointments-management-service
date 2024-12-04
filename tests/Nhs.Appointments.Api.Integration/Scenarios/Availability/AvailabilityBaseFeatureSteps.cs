using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Json;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;

public abstract class AvailabilityBaseFeatureSteps : BaseFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private QueryAvailabilityResponse _actualResponse;

    protected HttpResponseMessage Response => _response;
    protected HttpStatusCode StatusCode => _statusCode;
    protected QueryAvailabilityResponse ActualReponse => _actualResponse;

    [When(@"I check ([\w:]+) availability for '([\w:]+)' between '(.+)' and '(.+)'")]
    public async Task CheckAvailability(string queryType, string service, string from, string until)
    {
        var convertedQueryType = queryType switch
        {
            "daily" => QueryType.Days,
            "hourly" => QueryType.Hours,
            "slot" => QueryType.Slots,
            _ => throw new Exception($"{queryType} is not a valid queryType")
        };

        var payload = new
        {
            sites = new[] { GetSiteId() },
            service,
            from = ParseNaturalLanguageDateOnly(from),
            until = ParseNaturalLanguageDateOnly(until),
            queryType = convertedQueryType.ToString()
        };

        _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/availability/query", payload);
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<QueryAvailabilityResponse>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following availability is returned for '(.+)'")]
    [And(@"the following availability is returned for '(.+)'")]
    public void Assert(string date, Gherkin.Ast.DataTable expectedHourlyAvailabilityTable)
    {
        var expectedDate = ParseNaturalLanguageDateOnly(date);
        var expectedHourBlocks = expectedHourlyAvailabilityTable.Rows.Skip(1).Select(row =>
            new QueryAvailabilityResponseBlock(
                TimeOnly.ParseExact(row.Cells.ElementAt(0).Value, "HH:mm"),
                TimeOnly.ParseExact(row.Cells.ElementAt(1).Value, "HH:mm"),
                int.Parse(row.Cells.ElementAt(2).Value)
            ));

        var expectedAvailability = new QueryAvailabilityResponseInfo(
            expectedDate,
            expectedHourBlocks);

        _statusCode.Should().Be(HttpStatusCode.OK);
        _actualResponse
            .Single().availability
            .Single(x => x.date == expectedDate)
            .Should().BeEquivalentTo(expectedAvailability);
    }
    [When(@"I send an invalid availability query request")]
    public async Task SendInvalidAvailabilityQueryRequest()
    {
        var payload = new
        {
            sites = new[] { GetSiteId() },
            service = "",
            from = ParseNaturalLanguageDateOnly("Tomorrow"),
            until = ParseNaturalLanguageDateOnly("Tomorrow")
        };
        _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/availability/query", payload);
    }

    [Then(@"a bad request error is returned")]
    public void Assert()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}