using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class ProposePotentialUserRequestValidatorTests
{
    private readonly ProposePotentialUserRequestValidator _sut = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenSiteIsNullOrEmpty(string site)
    {
        var request = new ProposePotentialUserRequest(site, "test.user@nhs.net");

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(ProposePotentialUserRequest.SiteId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenUserIsNullOrEmpty(string user)
    {
        var request = new ProposePotentialUserRequest("34e990af-5dc9-43a6-8895-b9123216d699", user);

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(ProposePotentialUserRequest.UserId));
        result.Errors.Single().ErrorMessage.Should().Be("Provide an email address.");
    }

    [Theory]
    [InlineData("test@")]
    [InlineData("test@com")]
    [InlineData("test@test.")]
    [InlineData("test@testcom")]
    [InlineData("testtest.com")]
    [InlineData("@test.com")]
    public void Validate_ReturnsError_WhenUserIsNotValidEmailAddress(string user)
    {
        var request = new ProposePotentialUserRequest("34e990af-5dc9-43a6-8895-b9123216d699", user);

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(ProposePotentialUserRequest.UserId));
        result.Errors.Single().ErrorMessage.Should().Be("Provide a valid email address.");
    }

    [Fact]
    public void Validate_ReturnsError_WhenRequestIsEmpty()
    {
        ProposePotentialUserRequest request = null;

        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = new ProposePotentialUserRequest("34e990af-5dc9-43a6-8895-b9123216d699", "test.user@nhs.net");

        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
}
