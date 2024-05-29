using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class QueryBookingByNhsNumberRequestValidatorTests
{
    private readonly QueryBookingByNhsNumberRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenNshNumberIsBlank()
    {
        var testRequest = new QueryBookingByNhsNumberRequest(string.Empty);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryBookingByNhsNumberRequest.nhsNumber));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new QueryBookingByNhsNumberRequest("21334");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}

