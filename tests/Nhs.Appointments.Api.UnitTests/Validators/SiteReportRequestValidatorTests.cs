using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class SiteReportRequestValidatorTests
{
    private readonly SiteReportRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new SiteReportRequest(new DateOnly(2004, 2, 10), new DateOnly(2004, 2, 12));
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsError_WhenDateRangeIsInvalid()
    {
        var testRequest = new SiteReportRequest(new DateOnly(2004, 2, 15), new DateOnly(2004, 2, 12));
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SiteReportRequest.StartDate));
    }
}
