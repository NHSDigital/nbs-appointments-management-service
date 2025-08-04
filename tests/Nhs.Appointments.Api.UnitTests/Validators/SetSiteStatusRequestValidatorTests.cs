using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;
public class SetSiteStatusRequestValidatorTests
{
    private readonly SetSiteStatusRequestValidator _sut;

    public SetSiteStatusRequestValidatorTests()
    {
        _sut = new SetSiteStatusRequestValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FailsValidation_WhenSiteIsNullOrEmpty(string siteId)
    {
        var request = new SetSiteStatusRequest(siteId, SiteStatus.Offline);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors.First().ErrorMessage.Should().Be("Provide a valid site.");
    }

    [Fact]
    public void FailedValidation_WhenStatusIsNotInEnum()
    {
        var request = new SetSiteStatusRequest("site-id", (SiteStatus)20);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors.First().ErrorMessage.Should().Be("Provide a valid site status.");
    }

    [Fact]
    public void PassesValidation()
    {
        var request = new SetSiteStatusRequest("site-id", SiteStatus.Offline);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Count.Should().Be(0);
    }
}
