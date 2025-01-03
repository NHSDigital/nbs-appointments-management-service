using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;


namespace Nhs.Appointments.Api.Tests.Validators;

public class QueryBookingByReferenceRequestValidatorTests
{
    private readonly QueryBookingByReferenceRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenSiteIsBlank()
    {
        var testRequest = new QueryBookingByReferenceRequest("", string.Empty);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryBookingByReferenceRequest.bookingReference));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new QueryBookingByReferenceRequest("ref", string.Empty);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}
