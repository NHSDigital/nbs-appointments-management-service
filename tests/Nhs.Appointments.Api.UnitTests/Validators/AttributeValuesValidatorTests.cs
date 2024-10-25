using FluentAssertions;
using FluentValidation.TestHelper;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class AttributeValuesValidatorTests
{
    private readonly AttributeValueValidator _sut = new();
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenIdIsInvalid(string? id)
    {
        var attributeValue = new AttributeValue(
            Id: id,
            Value: "true");
        var result = _sut.TestValidate(attributeValue);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(AttributeValue.Id));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenValueIsInvalid(string? value)
    {
        var attributeValue = new AttributeValue(
            Id: "attribute_id",
            Value: value);
        var result = _sut.TestValidate(attributeValue);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(AttributeValue.Value));
    }
}
