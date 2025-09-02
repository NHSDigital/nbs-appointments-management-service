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
    public async Task QueryBookings(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);

        var fromDay = dataTable.GetRowValueOrDefault(row, "From (Day)");
        var fromTime = dataTable.GetRowValueOrDefault(row, "From (Time)");
        var toDay = dataTable.GetRowValueOrDefault(row, "Until (Day)");
        var toTime = dataTable.GetRowValueOrDefault(row, "Until (Time)");
        var statuses = dataTable.GetListRowValueOrDefault(row, "Statuses");
        var cancellationReason =
            dataTable.GetRowValueOrDefault(row, "Cancellation Reason");

        var cancellationNotificationStatuses =
            dataTable.GetListRowValueOrDefault(row,
                "Cancellation Notification Status");

        object payload = new
        {
            from = ToRequestFormat(fromDay, fromTime),
            to = ToRequestFormat(toDay, toTime),
            site = GetSiteId(),
            statuses,
            cancellationReason,
            cancellationNotificationStatuses
        };

        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/booking/query", payload);
        _statusCode = Response.StatusCode;
        (_, _actualResponse) =
            await JsonRequestReader.ReadRequestAsync<List<Core.Booking>>(
                await Response.Content.ReadAsStreamAsync());
    }

    private IEnumerable<Core.Booking> BuildBookingsFromDataTable(DataTable dataTable)
    {
        return dataTable.Rows.Skip(1).Select((row, index) =>
        {
            var bookingType = dataTable.GetEnumRowValue(row, "Booking Type", BookingType.Confirmed);
            var reference = CreateCustomBookingReference(dataTable.GetRowValueOrDefault(row, "Reference")) ??
                            BookingReferences.GetBookingReference(index, bookingType);
            var site = GetSiteId(dataTable.GetRowValueOrDefault(row, "Site", "beeae4e0-dd4a-4e3a-8f4d-738f9418fb51"));
            var service = dataTable.GetRowValueOrDefault(row, "Service", "RSV:Adult");
            var status = dataTable.GetEnumRowValue(row, "Status", AppointmentStatus.Booked);

            var day = dataTable.GetRowValueOrDefault(row, "Date", "Tomorrow");
            var time = dataTable.GetRowValueOrDefault(row, "Time", "10:00");
            var from = ParseDayAndTime(day, time);

            var createdDay = dataTable.GetRowValueOrDefault(row, "Created Day");
            var createdTime = dataTable.GetRowValueOrDefault(row, "Created Time");
            var created = createdDay != null && createdTime != null
                ? ParseDayAndTime(createdDay, createdTime)
                : GetCreationDateTime(bookingType);

            var duration = int.Parse(dataTable.GetRowValueOrDefault(row, "Duration", "10"));
            var availabilityStatus =
                dataTable.GetEnumRowValue(row, "Availability Status", MapAvailabilityStatus(bookingType));

            var cancellationReason = dataTable.GetEnumRowValueOrDefault<CancellationReason>(row, "Cancellation Reason");

            var booking = new Core.Booking
            {
                Reference = reference,
                From = from,
                Duration = duration,
                Service = service,
                Site = site,
                Status = status,
                AvailabilityStatus = availabilityStatus,
                CancellationReason = cancellationReason,
                Created = created,
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
            };

            return booking;
        });
    }

    [Then(@"the following bookings are returned")]
    public void Assert(DataTable expectedBookingDetailsTable)
    {
        var expectedBookings = BuildBookingsFromDataTable(expectedBookingDetailsTable);

        _statusCode.Should().Be(HttpStatusCode.OK);
        BookingAssertions.BookingsAreEquivalent(_actualResponse, expectedBookings);
    }
}
