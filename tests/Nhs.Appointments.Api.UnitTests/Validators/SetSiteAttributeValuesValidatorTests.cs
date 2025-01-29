using FluentAssertions;
using FluentValidation.TestHelper;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class SetSiteAttributeValuesValidatorTests
{
    private readonly SetSiteAttributeValuesValidator _sut = new();
    
    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void Validate_ReturnSuccess_WhenRequestIsValid(string value)
    {
        var testRequest = new SetSiteAccessibilitiesRequest(
            Site: "9a06bacd-e916-4c10-8263-21451ca751b8",
            Accessibilities: new[]
            {
                new Accessibility(
                    Id: "accessibility/attribute_1",
                    Value: value)
            });
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenSiteIsInvalid(string siteId)
    {
        var request = new SetSiteAccessibilitiesRequest(
            Site: siteId,
            Accessibilities: new[]
            {
                new Accessibility(
                    Id: "accessibility/attribute_1",
                    Value: "true"
                )
            }
        );
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SetSiteAccessibilitiesRequest.Site));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenAttributeValuesArrayIsNull()
    {
        var request = new SetSiteAccessibilitiesRequest(Site: "9a06bacd-e916-4c10-8263-21451ca751b8", Accessibilities: null);
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(SetSiteAccessibilitiesRequest.Accessibilities));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenAttributeValuesArrayIsEmpty()
    {
        var request = new SetSiteAccessibilitiesRequest(Site: "9a06bacd-e916-4c10-8263-21451ca751b8", Accessibilities: Array.Empty<Accessibility>());
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(SetSiteAccessibilitiesRequest.Accessibilities));
    }
}
