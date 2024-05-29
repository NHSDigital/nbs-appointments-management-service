using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class SiteBasedRequestValidatorTests
{
    private readonly SiteBasedResourceRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenSiteIsBlankAndUserIsFalse()
    {
        var testRequest = new SiteBasedResourceRequest("", false);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SiteBasedResourceRequest.Site));
    }

    [Fact]
    public void Validate_ReturnsError_WhenUserIsTrueAndSiteAlsoSpecified()
    {
        var testRequest = new SiteBasedResourceRequest("123", true);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SiteBasedResourceRequest.Site));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenSiteBasedRequestIsValid()
    {
        var testRequest = new SiteBasedResourceRequest("site", false);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenUserBasedRequestIsValid()
    {
        var testRequest = new SiteBasedResourceRequest("", true);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}
