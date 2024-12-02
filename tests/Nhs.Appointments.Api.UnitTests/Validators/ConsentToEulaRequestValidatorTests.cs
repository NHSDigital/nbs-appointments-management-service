using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class ConsentToEulaRequestValidatorTests
{
    private readonly ConsentToEulaRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = new ConsentToEulaRequest(new DateOnly(1983, 5, 7));

        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsError_WhenVersionDateIsNull()
    {
        var request = new ConsentToEulaRequest(default);
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(ConsentToEulaRequest.versionDate));
    }
}
