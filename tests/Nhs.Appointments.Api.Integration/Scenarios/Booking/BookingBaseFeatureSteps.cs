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
    
    [And(@"the original booking has been '(\w+)'")]
    public Task AssertRescheduledBookingStatus(string outcome)
    {
        return AssertBookingStatus(outcome);
    }
    
    [Then(@"the booking has been '(\w+)'")]
    public async Task AssertBookingStatus(string status)
    {
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertBookingStatusByReference(bookingReference, expectedStatus);
    }

    [Then(@"the booking with reference '(.+)' has been '(.+)'")]
    public async Task AssertSpecificBookingStatus(string bookingReference, string status)
    {
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);

        await AssertBookingStatusByReference(bookingReference, expectedStatus);
    }

    private async Task AssertBookingStatusByReference(string bookingReference, AppointmentStatus status)
    {
        var siteId = GetSiteId();
        var bookingDocument = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        bookingDocument.Resource.Status.Should().Be(status);
        bookingDocument.Resource.StatusUpdated.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));

        var indexDocument = await Client.GetContainer("appts", "index_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey("booking_index"));
        indexDocument.Resource.Status.Should().Be(status);
    }
    
    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);
}
