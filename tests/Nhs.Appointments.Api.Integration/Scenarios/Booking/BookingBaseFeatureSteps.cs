using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

public abstract class BookingBaseFeatureSteps : BaseFeatureSteps
{
    protected HttpResponseMessage Response { get; set; }
    
    [When("I make a provisional appointment with the following details")]
    public async Task MakeProvisionalBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        object payload = new
        {
            from = DateTime.ParseExact($"{ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {cells.ElementAt(1).Value}", "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm"),
            duration = cells.ElementAt(2).Value,
            service = cells.ElementAt(3).Value,
            site = GetSiteId(),
            kind = "provisional",
            attendeeDetails = new
            {
                nhsNumber = EvaluateNhsNumber(cells.ElementAt(4).Value),
                firstName = cells.ElementAt(5).Value,
                lastName = cells.ElementAt(6).Value,
                dateOfBirth = cells.ElementAt(7).Value
            }
        };

        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking", payload);
    }
    
    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);
}
