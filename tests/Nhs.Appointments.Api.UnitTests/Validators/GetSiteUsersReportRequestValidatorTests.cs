using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class GetSiteUsersReportRequestValidatorTests
{
    private readonly GetSiteUsersReportRequestValidator _sut = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void FailsValidation_WhenSiteIsNullOrEmpty(string site)
    {
        var request = new GetSiteUsersReportRequest(site);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.First().ErrorMessage.Should().Be("Site is required.");
    }

    [Fact]
    public void PassesValidation()
    {
        var request = new GetSiteUsersReportRequest("test-site-123");

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
