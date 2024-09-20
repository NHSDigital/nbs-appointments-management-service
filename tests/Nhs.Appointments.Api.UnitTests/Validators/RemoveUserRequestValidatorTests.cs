using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class RemoveUserRequestValidatorTests
{
    private readonly RemoveUserRequestValidator _sut = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenSiteIsNullOrEmpty(string? site)
    {
        var request = new RemoveUserRequest()
        {
            User = "test.user@nhs.net",
            Site = site
        };
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(RemoveUserRequest.Site));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("test@")]
    [InlineData("test@com")]
    [InlineData("test@test.")]
    [InlineData("test@testcom")]
    [InlineData("testtest.com")]
    [InlineData("@test.com")]
    public void Validate_ReturnsError_WhenUserIsNotValidEmailAddress(string? user)
    {
        var request = new RemoveUserRequest()
        {
            User = user,
            Site = "1001"
        };

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(RemoveUserRequest.User));
    }

    [Fact]
    public void Validate_ReturnsError_WhenRequestIsEmpty()
    {
        RemoveUserRequest request = null;

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = new RemoveUserRequest()
        {
            User = "test.user@nhs.net",
            Site = "1001"
        };

        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}
