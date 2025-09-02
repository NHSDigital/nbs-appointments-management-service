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
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Query;

[FeatureFile("./Scenarios/Booking/Query/QueryBookings.feature")]
public abstract class QueryBookingsFeatureSteps(string flag, bool enabled) : BookingBaseFeatureSteps(flag, enabled)
{
    private List<Core.Booking> _actualResponse;
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;

    [When("I query for bookings using the following parameters")]
    public async Task MakeBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        var fromDay = cells.ElementAt(0).Value;
        var fromTime = cells.ElementAt(1).Value;
        var toDay = cells.ElementAt(2).Value;
        var toTime = cells.ElementAt(3).Value;
        // var statuses = cells.ElementAtOrDefault(4)?.Value;
        // var cancellationReason = cells.ElementAtOrDefault(5)?.Value;
        // var cancellationNotificationStatuses = cells.ElementAtOrDefault(6)?.Value;

        object payload = new
        {
            from = ToRequestFormat(fromDay, fromTime), to = ToRequestFormat(toDay, toTime), site = GetSiteId()
        };

        // if (statuses is not null)
        // {
        //     payload.statuses = statuses.Split(',').Select(s => s.Trim()).ToArray();
        // }
        //
        // if (cancellationReason is not null)
        // {
        //     payload.statuses = cancellationReason;
        // }
        //
        // if (statuses is not null)
        // {
        //     payload.cancellationNotificationStatuses =
        //         cancellationNotificationStatuses.Split(',').Select(s => s.Trim()).ToArray();
        // }


        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/booking/query", payload);
        _statusCode = Response.StatusCode;
        (_, _actualResponse) =
            await JsonRequestReader.ReadRequestAsync<List<Core.Booking>>(
                await Response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following bookings are returned")]
    public void Assert(DataTable expectedBookingDetailsTable)
    {
        var expectedBookings = expectedBookingDetailsTable.Rows.Skip(1).Select((row, index) =>
            new Core.Booking
            {
                Reference =
                    CreateCustomBookingReference(row.Cells.ElementAtOrDefault(4)?.Value) ??
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

    // [Then(@"the request is successful and no bookings are returned")]
    // public void AssertNoAvailability()
    // {
    //     _statusCode.Should().Be(HttpStatusCode.OK);
    //     _actualResponse.Should().BeEmpty();
    // }
}
