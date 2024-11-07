using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

public abstract class BookingBaseFeatureSteps : BaseFeatureSteps
{
    protected HttpResponseMessage Response { get; set; }
    
    [When("I make a provisional appointment with the following details")]
    public async Task MakeProvisionalBooking(Gherkin.Ast.DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        object payload = new
        {
            from = cells.ElementAt(0).Value,
            duration = cells.ElementAt(1).Value,
            service = cells.ElementAt(2).Value,
            site = GetSiteId(),
            provisional = true,
            attendeeDetails = new
            {
                nhsNumber = EvaluateNhsNumber(cells.ElementAt(3).Value),
                firstName = cells.ElementAt(4).Value,
                lastName = cells.ElementAt(5).Value,
                dateOfBirth = cells.ElementAt(6).Value
            }
        };

        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking", payload);
    }
    
    [And(@"the original booking has been '(\w+)'")]
    public Task AssertRescheduledBookingOutcome(string outcome)
    {
        return AssertBookingOutcome(outcome);
    }
    
    [Then(@"the booking has been '(\w+)'")]
    public async Task AssertBookingOutcome(string outcome)
    { 
        Response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var siteId = GetSiteId();
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
        var actualResult = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));            
        actualResult.Resource.Outcome.Should().BeEquivalentTo(outcome);
    }
}
