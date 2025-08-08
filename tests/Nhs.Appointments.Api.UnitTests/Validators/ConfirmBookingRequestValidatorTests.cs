using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class ConfirmBookingRequestValidatorTests
{
    private readonly ConfirmBookingRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnError_WhenReferenceIsBlank()
    {
        var testRequest = new ConfirmBookingRequest(string.Empty, [new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org" }], null, string.Empty, null);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(ConfirmBookingRequest.bookingReference));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new ConfirmBookingRequest("my-ref", [new ContactItem{Type = ContactItemType.Email, Value = "test@tempuri.org"}], null, string.Empty, null);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}
