using FluentAssertions;
using FluentValidation.TestHelper;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class SetUserRoleValidatorTests
{
    private readonly SetUserRoleValidator _sut = new();

    [Fact]
    public void Validate_ReturnSuccess_WhenRequestIsValid()
    {
        var testRequest = new SetUserRolesRequest
        {
            Scope = "site:1000", User = "test.one@test.com", Roles = ["Role1", "Role2"]
        };
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")]
    [InlineData("site", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")]
    [InlineData(":", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")]
    [InlineData("site:", "")]
    public void Validate_ReturnError_WhenScopeIsNotValid(string prefix, string site)
    {
        var testRequest = new SetUserRolesRequest
        {
            Scope = prefix + site, User = "test@test.com", Roles = ["Role1", "Role2"]
        };
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SetUserRolesRequest.Scope));
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
    public void Validate_ReturnError_WhenUserIsNotValidEmailAddress(string user)
    {
        var testRequest = new SetUserRolesRequest { Scope = "site:1000", User = user, Roles = ["Role1", "Role2"] };
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SetUserRolesRequest.User));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", null)]
    [InlineData("", "2de5bb57-060f-4cb5-b14d-16587d0c2e8f")]
    public void Validate_ReturnsError_WhenRolesIsNullOrEmpty(params string[] roles)
    {
        var testRequest = new SetUserRolesRequest { Scope = "site:1000", User = "test@test.com", Roles = roles };
        var result = _sut.TestValidate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(SetUserRolesRequest.Roles));
    }

    [Fact]
    public void Validate_ReturnsError_WhenRolesArrayIsEmpty()
    {
        var testRequest = new SetUserRolesRequest
        {
            Scope = "site:1000", User = "test@test.com", Roles = Array.Empty<string>()
        };
        var result = _sut.TestValidate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(SetUserRolesRequest.Roles));
    }
}
