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
        var testRequest = new SetSiteAttributesRequest(
            Site: "9a06bacd-e916-4c10-8263-21451ca751b8",
            Scope: "*",
            AttributeValues: new[]
            {
                new AttributeValue(
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
        var request = new SetSiteAttributesRequest(
            Site: siteId,
            Scope: "*",
            AttributeValues: new[]
            {
                new AttributeValue(
                    Id: "accessibility/attribute_1",
                    Value: "true")
            });
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SetSiteAttributesRequest.Site));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenAttributeValuesArrayIsNull()
    {
        var request = new SetSiteAttributesRequest(Site: "9a06bacd-e916-4c10-8263-21451ca751b8", Scope: "*", AttributeValues: null);
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(SetSiteAttributesRequest.AttributeValues));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenAttributeValuesArrayIsEmpty()
    {
        var request = new SetSiteAttributesRequest(Site: "9a06bacd-e916-4c10-8263-21451ca751b8", Scope: "*", AttributeValues: Array.Empty<AttributeValue>());
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(SetSiteAttributesRequest.AttributeValues));
    }
}
