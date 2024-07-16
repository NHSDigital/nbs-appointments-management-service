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
        var testRequest = new SetUserRolesRequest()
        {
            Scope = "site:1000", 
            User = "test@test.com", 
            Roles = ["Role1", "Role2"]
        };
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnError_WhenScopeIsNullOrEmpty(string scope)
    {
        var testRequest = new SetUserRolesRequest()
        {
            Scope = scope, 
            User = "test@test.com", 
            Roles = ["Role1", "Role2"]
        };
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SetUserRolesRequest.Scope));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnError_WhenUserIsNullOrEmpty(string user)
    {
        var testRequest = new SetUserRolesRequest()
        {
            Scope = "site:1000", 
            User = user, 
            Roles = ["Role1", "Role2"]
        };
        var result = _sut.Validate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(SetUserRolesRequest.User));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1000", null)]
    [InlineData("", "1000")]
    public void Validate_ReturnsError_WhenRolesIsNullOrEmpty(params string[] roles)
    {
        var testRequest = new SetUserRolesRequest()
        {
            Scope = "site:1000",
            User = "test@test.com",
            Roles = roles
        };
        var result = _sut.TestValidate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(SetUserRolesRequest.Roles));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenRolesArrayIsEmpty()
    {
        var testRequest = new SetUserRolesRequest()
        {
            Scope = "site:1000",
            User = "test@test.com",
            Roles = Array.Empty<string>()
        };
        var result = _sut.TestValidate(testRequest);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(SetUserRolesRequest.Roles));
    }
}
