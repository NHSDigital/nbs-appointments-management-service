using System.Net;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly Mock<ISiteService> _siteService = new();

    public PermissionCheckerTests()
    {
        _sut = new PermissionChecker(
            _userAssignmentService.Object,
            _roleService.Object,
            _cache.Object,
            _siteService.Object);

        _cache.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(_cacheEntry.Object);
    }

    private Site GenerateSite(string id) =>
        new Site(
            id,
            "A",
            "Address",
            "PhoneNumber",
            "OdsCode",
            "Region",
            "Icb",
            "Info",
            new List<Accessibility>(),
            new Location("Location", new[] { 0.0 }),
            SiteStatus.Online);

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
    public async Task HasGlobalPermissions_ReturnsTrue_WhenPermissionAssigned()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["OtherPermission"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["TestPermission"] },
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "global" },
            new() { Role = "Role2", Scope = "global" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasGlobalPermissionAsync(userId, "TestPermission");
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task HasGlobalPermissions_ReturnsFalse_WhenPermissionNotAssigned()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["OtherPermission"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["DifferentPermission"] },
            new() { Id = "Role3", Name = "Role Three", Permissions = ["TestPermission"] },
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "global" },
            new() { Role = "Role2", Scope = "global" },
            new() { Role = "Role3", Scope = "site:test" },
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasGlobalPermissionAsync(userId, "TestPermission");
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task HasGlobalPermissions_ReturnsTrue_WhenPermissionAssignedMultiple()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission","OtherPermission"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["TestPermission","DifferentPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "global" },
            new() { Role = "Role2", Scope = "global" }
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.HasGlobalPermissionAsync(userId, "TestPermission");
        result.Should().BeTrue();
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
    public async Task GetSitesWithPermissionAsync_ReturnsNoSites_WhenUserHasNoSitesWithThePermission()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["OtherPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:test" },
        };

        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.GetSitesWithPermissionAsync(userId, "TestPermission");
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetSitesWithPermissionAsync_ReturnsOneSite_WhenUserHasASiteWithThePermission()
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
        var expectedSite = GenerateSite("test");
        var sites = new List<Site>() { expectedSite, GenerateSite("test-A"), GenerateSite("test-B"), GenerateSite("test-C") };

        _siteService.Setup(x => x.GetAllSites()).ReturnsAsync(sites);
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.GetSitesWithPermissionAsync(userId, "TestPermission");
        result.Should().BeEquivalentTo(new List<Site>() { expectedSite });
    }
    
    [Fact]
    public async Task GetSitesWithPermissionAsync_ReturnsMultipleSites_WhenUserHasMultipleSitesWithThePermission()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] },
            new() { Id = "Role2", Name = "Role Two", Permissions = ["TestPermission", "DifferentPermission"] },
            new() { Id = "Role3", Name = "Role Three", Permissions = ["TestPermission"] },
            new() { Id = "Role4", Name = "Role Four", Permissions = ["TestPermission", "OtherPermission"] },
            new() { Id = "Role5", Name = "Role Five", Permissions = ["OtherPermission"] },
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "site:test-A" },
            new() { Role = "Role2", Scope = "site:test-B" },
            new() { Role = "Role3", Scope = "site:test-A" },
            new() { Role = "Role4", Scope = "site:test-C" },
            new() { Role = "Role5", Scope = "site:test-D" },
        };
        var sites = new List<Site>() { GenerateSite("test-A"), GenerateSite("test-B"), GenerateSite("test-C") };

        _siteService.Setup(x => x.GetAllSites()).ReturnsAsync(sites);
        _roleService.Setup(x => x.GetRoles()).ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId)).ReturnsAsync(userAssignments);
        var result = await _sut.GetSitesWithPermissionAsync(userId, "TestPermission");
        result.Should().BeEquivalentTo(sites);
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

    [Fact]
    public async Task HasPermissionAsync_ReturnsTrue_WhenUserHasRegionOnlyLevelPermission()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "region:R1" }
        };

        _roleService.Setup(x => x.GetRoles())
            .ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId))
            .ReturnsAsync(userAssignments);
        _siteService.Setup(x => x.GetSitesInRegion(It.IsAny<string>()))
            .ReturnsAsync(new List<Site>
            {
                new("1", "TestSite", "TestAddress", "N", "T1", "R1", "ICB", string.Empty, [], new Location("Test", [0,0]), SiteStatus.Online)
            });

        var result = await _sut.HasPermissionAsync(userId, ["1"], "TestPermission");

        result.Should().BeTrue();

        _siteService.Verify(s => s.GetSitesInRegion(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task GetPermissionsAsync_FiltersPermissionsOnRegion()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "region:R1" },
        };

        _roleService.Setup(x => x.GetRoles())
            .ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId))
            .ReturnsAsync(userAssignments);
        _siteService.Setup(x => x.GetSitesInRegion(It.IsAny<string>()))
            .ReturnsAsync(new List<Site>
            {
                new("2", "TestSite", "TestAddress", "N", "T1", "R1", "ICB", string.Empty, [], new Location("Test", [0,0]), SiteStatus.Online)
            });

        var result = await _sut.GetPermissionsAsync(userId, "2");

        result.Count().Should().Be(1);
        result.First().Should().Be("TestPermission");

        _siteService.Verify(s => s.GetSitesInRegion(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task GetRegionPermissionsAsync_ShouldReturnRegionOnlyPermissions_AndFilterDuplicates()
    {
        const string userId = "test@test.com";
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "region:R1" },
            new() { Role = "Role1", Scope = "region:R1" },
            new() { Role = "Role2", Scope = "site:1" },
            new() { Role = "Role3", Scope = "global" }
        };

        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId))
            .ReturnsAsync(userAssignments);

        var result = await _sut.GetRegionPermissionsAsync(userId);

        result.Count(r => r == "region:R1").Should().Be(1);
        result.First().Should().Be("region:R1");
    }

    [Fact]
    public async Task GetPermissionsAsync_FiltersPermissionsOnIcb()
    {
        const string userId = "test@test.com";
        var roles = new List<Role>
        {
            new() { Id = "Role1", Name = "Role One", Permissions = ["TestPermission"] }
        };
        var userAssignments = new List<RoleAssignment>
        {
            new() { Role = "Role1", Scope = "icb:ICB1" },
        };

        _roleService.Setup(x => x.GetRoles())
            .ReturnsAsync(roles);
        _userAssignmentService.Setup(x => x.GetUserRoleAssignments(userId))
            .ReturnsAsync(userAssignments);
        _siteService.Setup(x => x.GetSitesInIcbAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Site>
            {
                new("2", "TestSite", "TestAddress", "N", "T1", "R1", "ICB1", string.Empty, [], new Location("Test", [0,0]), SiteStatus.Online)
            });

        var result = await _sut.GetPermissionsAsync(userId, "2");

        result.Count().Should().Be(1);
        result.First().Should().Be("TestPermission");

        _siteService.Verify(s => s.GetSitesInIcbAsync(It.IsAny<string>()), Times.Once());
    }
}
