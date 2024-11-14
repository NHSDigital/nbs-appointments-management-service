using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/ApplyAvailabilityTemplate.feature")]
    public sealed class ApplyAvailabilityTemplateFeatureSteps : BaseFeatureSteps
    {
        private HttpResponseMessage _response;
        private HttpStatusCode _statusCode;
        private EmptyResponse _actualResponse;
        
        [Given("there is no existing availability")]
        public Task NoAvailability()
        {
            return Task.CompletedTask; // Could we clear out any existing
        }

        [When("I apply the following availability template")]
        public async Task ApplyTemplate(Gherkin.Ast.DataTable dataTable)
        {
            var cells = dataTable.Rows.ElementAt(1).Cells;
            var site = GetSiteId();
            var fromDate = DeriveRelativeDateOnly(cells.ElementAt(0).Value);
            var untilDate = DeriveRelativeDateOnly(cells.ElementAt(1).Value);
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



            var request = new ApplyAvailabilityTemplateRequest(site, fromDate.ToString("yyyy-MM-dd"), untilDate.ToString("yyyy-MM-dd"), template);
            var payload = JsonResponseWriter.Serialize(request);
            _response = await Http.PostAsync($"http://localhost:7071/api/availability/apply-template", new StringContent(payload));
            _statusCode = _response.StatusCode;
            (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<EmptyResponse>(await _response.Content.ReadAsStreamAsync());
        }
            
        [Then("the request is successful and the following daily availability is created")]
        public async Task AssertDailyAvailability(Gherkin.Ast.DataTable expectedDailyAvailabilityTable)
        {
            _statusCode.Should().Be(HttpStatusCode.OK);
            var site = GetSiteId();
            var expectedDocuments = DailyAvailabilityDocumentsFromTable(site, expectedDailyAvailabilityTable);
            var container = Client.GetContainer("appts", "booking_data");
            var actualDocuments = await RunQueryAsync<DailyAvailabilityDocument>(container, d => d.DocumentType == "daily_availability" && d.Site == site);
            actualDocuments.Should().BeEquivalentTo(expectedDocuments);
        }
    }
}
