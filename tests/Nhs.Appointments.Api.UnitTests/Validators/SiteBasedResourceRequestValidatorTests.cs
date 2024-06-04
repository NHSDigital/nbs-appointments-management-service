using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class SiteBasedRequestValidatorTests
{
    private readonly SiteBasedResourceRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenSiteIsBlank()
    {
        var testRequest = new SiteBasedResourceRequest("");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SiteBasedResourceRequest.Site));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenSiteBasedRequestIsValid()
    {
        var testRequest = new SiteBasedResourceRequest("site");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }    
}
