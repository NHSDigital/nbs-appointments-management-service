using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class GetRolesValidatorTests
{
    private readonly GetRolesRequestValidator _sut = new();

    [Fact]
    public void Validate_ReturnsError_WhenNshNumberIsBlank()
    {
        var testRequest = new GetRolesRequest(string.Empty);
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(GetRolesRequest.tag));
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var testRequest = new GetRolesRequest("canned");
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}

