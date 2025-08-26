using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class ConfirmBookingRequestValidatorTests
{
    private readonly ConfirmBookingRequestValidator _sut = new();
    private readonly ContactItem _mockContactInfo = new() { Type = ContactItemType.Email, Value = "test@tempuri.org" };

    [Fact]
    public void Validate_ReturnError_WhenReferenceIsBlank()
    {
        var testRequest = new ConfirmBookingRequest(string.Empty, [_mockContactInfo], null, null);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(ConfirmBookingRequest.bookingReference));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new ConfirmBookingRequest("my-ref", [_mockContactInfo], null, null);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValidWithRelatedBookings()
    {
        var testRequest =
            new ConfirmBookingRequest("my-ref", [_mockContactInfo], ["some-ref", "some-other-ref"], null);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValidWithBookingToReschedule()
    {
        var testRequest =
            new ConfirmBookingRequest("my-ref", [_mockContactInfo], ["some-ref", "some-other-ref"],
                "some-ref-to-reschedule");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void Validate_ReturnsError_WhenRelatedBookingsIncludesThePrimaryBooking()
    {
        var testRequest = new ConfirmBookingRequest("my-ref",
            [_mockContactInfo], ["some-ref", "my-ref", "some-other-ref"], null);

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().ErrorMessage.Should()
            .Be("Related bookings must not include the primary booking reference");
    }

    [Fact]
    public void Validate_ReturnsError_WhenRelatedBookingsIncludesTheBookingToReschedule()
    {
        var testRequest = new ConfirmBookingRequest("my-ref",
            [_mockContactInfo], ["some-ref", "some-other-ref", "some-ref-to-reschedule"],
            "some-ref-to-reschedule");

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().ErrorMessage.Should()
            .Be("Related bookings must not include the booking to reschedule");
    }

    [Fact]
    public void Validate_ReturnsError_WhenRelatedBookingsIncludesAnEmptyBooking()
    {
        var testRequest = new ConfirmBookingRequest("my-ref",
            [_mockContactInfo], [""], null);

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().ErrorMessage.Should()
            .Be("Each related booking must be a valid booking reference");
    }

    [Fact]
    public void Validate_ReturnsError_WhenRelatedBookingsIncludesANullBooking()
    {
        var testRequest = new ConfirmBookingRequest("my-ref",
            [_mockContactInfo], [""], null);

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().ErrorMessage.Should()
            .Be("Each related booking must be a valid booking reference");
    }

    [Fact]
    public void Validate_ReturnsError_WhenRelatedBookingsIncludesDuplicateBookings()
    {
        var testRequest = new ConfirmBookingRequest("my-ref",
            [_mockContactInfo], ["some-ref", "some-ref"], null);

        var result = _sut.Validate(testRequest);

        result.IsValid.Should().BeFalse();
        result.Errors.Single().ErrorMessage.Should()
            .Be("Related bookings must not include the same booking more than once");
    }
}
