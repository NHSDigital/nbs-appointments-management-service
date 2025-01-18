using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class MakeBookingRequestValidatorTests
{
    private readonly MakeBookingRequestValidator _sut = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenSiteIsNullOrEmpty(string site)
    {
        var request = new MakeBookingRequest(
            site,
            new DateTime(2077, 01, 01, 09, 0, 0),
            5,
            "COVID",
            GetAttendeeDetails(),
            GetContactDetails(),
            null,
            BookingKind.Booked
        );

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.Site));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(301)]
    public void Validate_ReturnsError_WhenDurationIsOutOfRange(int duration)
    {
        var request = new MakeBookingRequest(
            "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
            new DateTime(2077, 01, 01, 09, 0, 0),
            duration,
            "COVID",
            GetAttendeeDetails(),
            GetContactDetails(),
            null,
            BookingKind.Booked
        );

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.Duration));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenServiceIsNullOrEmpty(string service)
    {
        var request = new MakeBookingRequest(
            "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
            new DateTime(2077, 01, 01, 09, 0, 0),
            5,
            service,
            GetAttendeeDetails(),
            GetContactDetails(),
            null,
            BookingKind.Booked
        );

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.Service));
    }

    [Fact]
    public void Validate_ReturnsError_WhenAttendeeDetailsIsNull()
    {
        var request = new MakeBookingRequest(
            "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
            new DateTime(2077, 01, 01, 09, 0, 0),
            5,
            "COVID",
            null,
            GetContactDetails(),
            null,
            BookingKind.Booked
        );

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(MakeBookingRequest.AttendeeDetails));
    }

    [Fact]
    public void Validate_ReturnsError_WhenRequestIsEmpty()
    {
        MakeBookingRequest request = null;

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = new MakeBookingRequest(
            "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
            new DateTime(2077, 01, 01, 09, 0, 0),
            5,
            "COVID",
            GetAttendeeDetails(),
            GetContactDetails(),
            null,
            BookingKind.Booked
        );
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    private AttendeeDetails GetAttendeeDetails()
    {
        var attendeeDetails = new AttendeeDetails
        {
            NhsNumber = "1234567890",
            FirstName = "FirstName",
            LastName = "LastName",
            DateOfBirth = new DateOnly(1980, 01, 01)
        };
        return attendeeDetails;
    }

    private ContactItem[] GetContactDetails()
    {
        return
        [
            new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org" },
            new ContactItem { Type = ContactItemType.Phone, Value = "0123456789" }
        ];
    }
}
