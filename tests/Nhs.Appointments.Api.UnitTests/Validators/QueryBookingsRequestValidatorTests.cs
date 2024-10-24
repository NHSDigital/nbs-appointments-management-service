using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class QueryBookingsRequestValidatorTests
{
    private readonly QueryBookingsRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnError_WhenSiteIsBlank()
    {
        var testRequest = new QueryBookingsRequest(DateTime.Today, DateTime.Today.AddDays(1), string.Empty);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryBookingsRequest.site));
    }

    [Fact]
    public void Validate_ReturnError_WhenDateRangeIsInvalid()
    {
        var testRequest = new QueryBookingsRequest(DateTime.Today.AddDays(1), DateTime.Today, "site");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryBookingsRequest.from));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new QueryBookingsRequest(DateTime.Today, DateTime.Today.AddDays(1), "site");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}
