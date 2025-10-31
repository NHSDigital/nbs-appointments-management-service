using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    public abstract class QueryBookingByNhsNumberFeatureSteps(string flag, bool enabled) : BookingBaseFeatureSteps(flag, enabled)
    {
        private List<Core.Booking> _actualResponse;
        private HttpResponseMessage _response;
        private HttpStatusCode _statusCode;

        [When(@"I query for bookings for a person using their NHS number")]
        public async Task CheckAvailability()
        {
            _response = await Http.GetAsync($"http://localhost:7071/api/booking?nhsNumber={NhsNumber}");
            _statusCode = _response.StatusCode;
            (_, _actualResponse) =
                await JsonRequestReader.ReadRequestAsync<List<Core.Booking>>(
                    await _response.Content.ReadAsStreamAsync());
        }

        // TODO: Need to add this to the base class along with new tests for QueryByReference
        [Then(@"the following bookings are returned")]
        public void Assert(DataTable expectedBookingDetailsTable)
        {
            var expectedBookings = expectedBookingDetailsTable.Rows.Skip(1).Select((row, index) =>
                new Core.Booking
                {
                    Reference =
                        row.Cells.ElementAtOrDefault(4)?.Value ??
                        BookingReferences.GetBookingReference(index, BookingType.Confirmed),
                    From =
                        DateTime.ParseExact(
                            $"{ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
                            "yyyy-MM-dd HH:mm", null),
                    Duration = int.Parse(row.Cells.ElementAt(2).Value),
                    Service = row.Cells.ElementAt(3).Value,
                    Site = GetSiteId(),
                    Created = GetCreationDateTime(BookingType.Confirmed),
                    Status = AppointmentStatus.Booked,
                    AttendeeDetails = new AttendeeDetails
                    {
                        NhsNumber = NhsNumber,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        DateOfBirth = new DateOnly(2000, 1, 1)
                    },
                    ContactDetails =
                    [
                        new ContactItem { Type = ContactItemType.Email, Value = GetContactInfo(ContactItemType.Email) },
                        new ContactItem { Type = ContactItemType.Phone, Value = GetContactInfo(ContactItemType.Phone) },
                        new ContactItem
                        {
                            Type = ContactItemType.Landline, Value = GetContactInfo(ContactItemType.Landline)
                        }
                    ],
                    AdditionalData = new { IsAppBooking = true }
                }).ToList();

            _statusCode.Should().Be(HttpStatusCode.OK);
            BookingAssertions.BookingsAreEquivalent(_actualResponse, expectedBookings);
        }

        [Then(@"the request is successful and no bookings are returned")]
        public void AssertNoAvailability()
        {
            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.Should().BeEmpty();
        }
    }
    
    
    [Collection("BookingReferenceV2SerialToggle")]
    [FeatureFile("./Scenarios/Booking/QueryBookingByNhsNumber.feature")]
    public class QueryBookingByNhsNumber_BookingReferenceV2Enabled()
        : QueryBookingByNhsNumberFeatureSteps(Flags.BookingReferenceV2, true);

    [Collection("BookingReferenceV2SerialToggle")]
    [FeatureFile("./Scenarios/Booking/QueryBookingByNhsNumber.feature")]
    public class QueryBookingByNhsNumber_BookingReferenceV2Disabled() 
        : QueryBookingByNhsNumberFeatureSteps(Flags.BookingReferenceV2, false);
}
