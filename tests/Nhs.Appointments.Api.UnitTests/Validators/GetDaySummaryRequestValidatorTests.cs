using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;
public class GetDaySummaryRequestValidatorTests
{
    private readonly GetDaySummaryRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenSiteIsBlank()
    {
        var testRequest = new GetDaySummaryRequest(string.Empty, "2025-12-12");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetDaySummaryRequest.Site));
    }

    [Fact]
    public void Validate_ReturnsError_WhenFromIsBlank()
    {
        var testRequest = new GetDaySummaryRequest("Site01", string.Empty);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetDaySummaryRequest.From));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new GetDaySummaryRequest("Site01", "2025-12-12");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}


