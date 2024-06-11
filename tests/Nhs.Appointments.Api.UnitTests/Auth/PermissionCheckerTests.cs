using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Auth;

public class PermissionCheckerTests
{
    private readonly PermissionChecker _sut;
    private readonly Mock<IUserSiteAssignmentService> _userAssignmentService = new();
    private readonly Mock<IRolesService> _roleService = new();

    public PermissionCheckerTests()
    {
        _sut = new PermissionChecker(_userAssignmentService.Object, _roleService.Object);
    }

    [Fact]
    public async Task HasPermissionAsync_ReturnsFalse_UserHasNoRoles()
    {
        const string userId = "test@test.com";
        const string requiredPermission = "Permission1";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission1", "Permission2"] }
        };
        var userAssignments = new List<UserAssignment>
        {
            new() { Email = userId, Site = "1000", Roles = null }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserAssignedSites(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, requiredPermission);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_ReturnsFalse_WhenUserDoesNotHaveRequiredPermission()
    {
        const string userId = "test@test.com";
        const string requiredPermission = "Permission3";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission1", "Permission2"] }
        };
        var userAssignments = new List<UserAssignment>
        {
            new() { Email = userId, Site = "__global__", Roles = [ "Role1" ] }
        };
        
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserAssignedSites(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, requiredPermission);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_ReturnsTrue_WhenUserHasRequiredPermission()
    {
        const string userId = "test@test.com";
        const string requiredPermission = "Permission1";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission1", "Permission2"] }
        };
        var userAssignments = new List<UserAssignment>
        {
            new() { Email = userId, Site = "__global__", Roles = [ "Role1" ] }
        };
        
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserAssignedSites(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, requiredPermission);
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task HasPermissionAsync_ReturnsTrue_WhenCheckingForRequiredPermissionAcrossMultipleRoles()
    {
        const string userId = "test@test.com";
        const string requiredPermission = "Permission-4";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission-1", "Permission-2"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["Permission-3", "Permission-4"] }
        };
        var userAssignments = new List<UserAssignment>
        {
            new() { Email = userId, Site = "__global__", Roles = [ "Role1", "Role2" ] }
        };
        
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserAssignedSites(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, requiredPermission);
        result.Should().BeTrue();
    }
}
