using FluentAssertions;
using FluentValidation.TestHelper;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class AccessibilityValidatorTests
{
    private readonly AccessibilityValidator _sut = new();
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenIdIsInvalid(string id)
    {
        var accessibility = new Accessibility(
            Id: id,
            Value: "true");
        var result = _sut.TestValidate(accessibility);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Accessibility.Id));
    }
}
