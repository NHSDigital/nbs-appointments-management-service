using FluentAssertions;

namespace CosmosDbSeederTests;

public class GlobalRolesTests : BaseCosmosDbSeederTest
{
    [Fact]
    public void GlobalRolesShouldBeTheSameInEachEnvironment()
    {
        var localGlobalRoles = ReadDocument<GlobalRolesDocument>("local");
        var devGlobalRoles = ReadDocument<GlobalRolesDocument>("dev");
        var intGlobalRoles = ReadDocument<GlobalRolesDocument>("int");
        var penGlobalRoles = ReadDocument<GlobalRolesDocument>("pen");
        var perfGlobalRoles = ReadDocument<GlobalRolesDocument>("perf");
        var stagGlobalRoles = ReadDocument<GlobalRolesDocument>("stag");
        var prodGlobalRoles = ReadDocument<GlobalRolesDocument>("prod");

        devGlobalRoles.Should().BeEquivalentTo(intGlobalRoles);
        intGlobalRoles.Should().BeEquivalentTo(penGlobalRoles);
        penGlobalRoles.Should().BeEquivalentTo(perfGlobalRoles);
        perfGlobalRoles.Should().BeEquivalentTo(stagGlobalRoles);
        stagGlobalRoles.Should().BeEquivalentTo(prodGlobalRoles);

        localGlobalRoles.Roles =
            localGlobalRoles.Roles.Where(role => role.Id != "system:integration-test-user").ToArray();
        localGlobalRoles.Should().BeEquivalentTo(devGlobalRoles);
    }

    [Fact(DisplayName = "APPT-802: Admin User roles should include user manager")]
    public void AdminUserRolesShouldIncludeManageUsers()
    {
        var adminRoles = ReadDocument<GlobalRolesDocument>("local").Roles
            .Single(role => role.Id == "system:admin-user");

        adminRoles.Permissions.Should().Contain("users:manage");
    }
}
