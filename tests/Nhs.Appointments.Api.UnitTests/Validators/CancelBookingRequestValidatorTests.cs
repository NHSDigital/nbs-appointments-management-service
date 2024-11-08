using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class CancelBookingRequestValidatorTests
{
    private readonly CancelBookingRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnError_WhenBookingReferenceIsBlank()
    {
        var testRequest = new CancelBookingRequest(string.Empty);            
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(CancelBookingRequest.bookingReference));
    }

    [Fact]
    public void Validate_ReturnsTrue_WhenRequestIsValid()
    {
        var testRequest = new CancelBookingRequest("ref");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}
