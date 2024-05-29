using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class FindSitesByPostCodeRequestValidatorTests
{
    private readonly FindSitesByPostCodeRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnError_WhenPostcodeIsBlank()
    {
        var testRequest = new FindSitesByPostCodeRequest("");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(FindSitesByPostCodeRequest.postCode));
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("word")]
    [InlineData("DFG128adj")]
    [InlineData("DF12812FD")]
    public void Validate_ReturnError_WhenPostcodeFormatIsIncorrect(string postcode)
    {
        var testRequest = new FindSitesByPostCodeRequest(postcode);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(FindSitesByPostCodeRequest.postCode));
    }

    [Theory]
    [InlineData("LS132AJ")]
    [InlineData("LS12AJ")]
    [InlineData("S12AJ")]
    public void Validate_ReturnsSuccess_WhenPostcodeIsValid(string postcode)
    {
        var testRequest = new FindSitesByPostCodeRequest(postcode);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}

