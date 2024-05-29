using FluentAssertions;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class SiteConfigurationValidatorTests
{
    private readonly SiteConfigurationValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenRequestIsMissingData()
    {
        var request = new SiteConfiguration();
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }
    
    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = new SiteConfiguration
        {
            SiteId = "26"
        };
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}