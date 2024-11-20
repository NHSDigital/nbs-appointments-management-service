using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability;

public abstract class BaseCreateAvailabilityFeatureSteps : BaseFeatureSteps
{
    protected readonly List<AvailabilityCreatedEvent> _expectedAvailabilityCreatedEvents = [];
    protected HttpResponseMessage _response;
    protected HttpStatusCode _statusCode;
    protected EmptyResponse _actualResponse;

    [Given("there is no existing availability")]
    public Task NoAvailability()
    {
        return Task.CompletedTask; // Could we clear out any existing
    }

    [And(@"the following availability created events are created")]
    [Then(@"the following availability created events are created")]
    public async Task AssertAvailabilityCreatedEventsAsync(DataTable dataTable)
    {
        PopulateExpectedAvailabilityCreatedEventsFromTable(dataTable);

        var actualAvailabilityCreatedEvents = await GetActualAvailabilityCreatedEvents();

        actualAvailabilityCreatedEvents.Should().BeEquivalentTo(_expectedAvailabilityCreatedEvents,
            options => options.Excluding(
                x => x.Created));
    }

    private void PopulateExpectedAvailabilityCreatedEventsFromTable(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows.Skip(1))
        {
            if (row.Location.Line == 0)
            {
                continue;
            }

            var cells = row.Cells.ToList();
            var type = cells.ElementAt(0).Value;
            var by = cells.ElementAt(1).Value;
            var fromDate = ParseNaturalLanguageDateOnly(cells.ElementAt(2).Value);
            var toDate = string.IsNullOrWhiteSpace(cells.ElementAt(3).Value) ? default : ParseNaturalLanguageDateOnly(cells.ElementAt(3).Value);
            var templateDays = DeriveWeekDaysInRange(fromDate, toDate);
            var fromTime = cells.ElementAt(5).Value;
            var untilTime = cells.ElementAt(6).Value;
            var slotLength = cells.ElementAt(7).Value;
            var capacity = cells.ElementAt(8).Value;
            var services = cells.ElementAt(9).Value;

            var session = new Session()
            {
                From = TimeOnly.Parse(fromTime),
                Until = TimeOnly.Parse(untilTime),
                SlotLength = int.Parse(slotLength),
                Capacity = int.Parse(capacity),
                Services = services.Split(',').Select(s => s.Trim()).ToArray()
            };

            var template = type == "Template"
                ? new Template()
                {
                    Days = ParseDays(templateDays),
                    Sessions = [session]
                } : null;

            var expectedEvent = new AvailabilityCreatedEvent()
            {
                Created = DateTime.UtcNow,
                By = by,
                Site = GetSiteId(),
                From = fromDate,
                To = type == "Template" ? toDate : null,
                Template = template,
                Sessions = type == "SingleDateSession" ? [session] : null
            };

            _expectedAvailabilityCreatedEvents.Add(expectedEvent);
        }
    }

    private async Task<List<AvailabilityCreatedEventDocument>> GetActualAvailabilityCreatedEvents()
    {
        var siteId = GetSiteId();

        var container = Client.GetContainer("appts", "booking_data");
        var actualDocuments =
            await RunQueryAsync<AvailabilityCreatedEventDocument>(
                container,
                d => d.DocumentType == "availability_created_event"
                     && d.Site == siteId);
        return actualDocuments.ToList();
    }

    [When("I apply the following availability template")]
    [And("I apply the following availability template")]
    public async Task ApplyTemplate(Gherkin.Ast.DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;
        var site = GetSiteId();
        var fromDate = ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value);
        var untilDate = ParseNaturalLanguageDateOnly(cells.ElementAt(1).Value);
        var days = DeriveWeekDaysInRange(fromDate, untilDate);

        var template = new Template
        {
            Days = ParseDays(days),
            Sessions = new[]
            {
                new Session
                {
                    From = TimeOnly.Parse(cells.ElementAt(3).Value),
                    Until = TimeOnly.Parse(cells.ElementAt(4).Value),
                    SlotLength = int.Parse(cells.ElementAt(5).Value),
                    Capacity = int.Parse(cells.ElementAt(6).Value),
                    Services = cells.ElementAt(7).Value.Split(",").Select(s => s.Trim()).ToArray()
                }
            }
        };

        var request = new ApplyAvailabilityTemplateRequest(site, fromDate, untilDate, template);
        var payload = JsonResponseWriter.Serialize(request);
        _response = await Http.PostAsync($"http://localhost:7071/api/availability/apply-template", new StringContent(payload));
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<EmptyResponse>(await _response.Content.ReadAsStreamAsync());
    }

    [When(@"I apply the following availability")]
    [And(@"I apply the following availability")]
    public async Task SetAvailability(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        var relativeDate = ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd");
        var payload = new
        {
            date = relativeDate,
            site = GetSiteId(),
            sessions = new[]
            {
                new {
                    from = cells.ElementAt(1).Value,
                    until = cells.ElementAt(2).Value,
                    slotLength = cells.ElementAt(3).Value,
                    capacity = cells.ElementAt(4).Value,
                    services = cells.ElementAt(5).Value.Split(',').Select(s => s.Trim()).ToArray(),
                }
            }
        };

        _response = await Http.PostAsJsonAsync("http://localhost:7071/api/availability", payload);
    }
}
