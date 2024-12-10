using System.Net;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Auth;

public class PermissionCheckerTests
{
    private readonly PermissionChecker _sut;
    private readonly Mock<IUserService> _userAssignmentService = new();
    private readonly Mock<IRolesService> _roleService = new();
    private readonly Mock<IMemoryCache> _cache = new();
    private readonly Mock<ICacheEntry> _cacheEntry = new();

    public PermissionCheckerTests()
    {
        _sut = new PermissionChecker(_userAssignmentService.Object, _roleService.Object, _cache.Object);
        _cache.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(_cacheEntry.Object);
    }

    [Fact]
    public async Task HasPermissions_ReturnsTrue_WhenPermissionAssignedGloballyAndGeneralRequest()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "global" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, [], "TestPermission");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissions_ReturnsTrue_WhenPermissionAssignedGloballyAndSiteSpecific()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "global" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, ["test"], "TestPermission");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissions_ReturnsTrue_WhenPermissionAssignedForSite()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:test" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, ["test"], "TestPermission");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissions_ReturnsFalse_WhenPermissionNotAssigned()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:test" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, ["test"], "OtherPermission");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissions_ReturnsFalse_WhenPermissionAssignedButForDifferentSite()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:other" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, ["test"], "OtherPermission");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissions_ReturnsTrue_MultiSiteRequest_Scoped()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:test" },
            new() { Role = "Role1", Scope = "site:other" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, ["test", "other"], "TestPermission");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissions_ReturnsTrue_MultiSiteRequest_Global()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "global" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, ["test", "other"], "TestPermission");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissions_ReturnsTrue_MultiSiteRequest_ScopedRequiresAll()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:test" },
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasPermissionAsync(userId, ["test", "other"], "TestPermission");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPermissionAsync_ReturnsNoPermissions_WhenUserHasNoRoles()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission1", "Permission2"] }
        };
        
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ThrowsAsync(new CosmosException("Resource not found", HttpStatusCode.NotFound, 0, "1", 1));
        var result = await _sut.GetPermissionsAsync(userId, "1");
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetPermissionAsync_ReturnsNoPermissions_WhenThereAreNoRoles()
    {
        const string userId = "test@test.com";
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "global" }
        };
        _roleService.Setup(x => x.GetRoles()).ThrowsAsync(new CosmosException("Resource not found", HttpStatusCode.NotFound, 0, "1", 1));
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.GetPermissionsAsync(userId, "1");
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetPermissionAsync_ReturnsAllPermissions_ForAllUserAssignedRoles()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission-1", "Permission-2"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["Permission-3", "Permission-4"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:1" },
            new() { Role = "Role2", Scope = "site:1" }
        };

        var expectedResult = new string[] { "Permission-1", "Permission-2", "Permission-3", "Permission-4" };
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.GetPermissionsAsync(userId, "1");
        result.Should().Equal(expectedResult);
    }
    
    [Fact]
    public async Task GetPermissionAsync_OnlyReturnsPermissions_ForUserAssignedRolesAtRequestedSiteOrGlobalScope()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission-1", "Permission-2"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["Permission-3", "Permission-4"] },
            new() { Id = "Role3", Name = "Role Three", Permissions = ["Permission-5", "Permission-6"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:1" },
            new() { Role = "Role2", Scope = "site:2" },
            new() { Role = "Role3", Scope = "global" }
        };

        var expectedResult = new [] { "Permission-1", "Permission-2", "Permission-5", "Permission-6" };
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.GetPermissionsAsync(userId, "1");
        result.Should().Equal(expectedResult);
    }
  
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetPermissionAsync_OnlyReturnsPermissionsForGlobalUserAssignedRoles_IfSiteIdIsNullOrEmpty(string siteId)
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission-1", "Permission-2"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["Permission-3", "Permission-4"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:1" },
            new() { Role = "Role2", Scope = "global" }
        };

        var expectedResult = new string[] { "Permission-3", "Permission-4" };
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.GetPermissionsAsync(userId, siteId);
        result.Should().Equal(expectedResult);
    }
    
    [Fact]
    public async Task GetPermissionAsync_OnlyReturnsDistinctPermissions_WhenUserAssignedRolesContainOverlappingPermissions()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["Permission-1", "Permission-2"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["Permission-2", "Permission-3"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:1" },
            new() { Role = "Role2", Scope = "site:1" }
        };

        var expectedResult = new [] { "Permission-1", "Permission-2", "Permission-3" };
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.GetPermissionsAsync(userId, "1");
        result.Should().Equal(expectedResult);
    }    
}
