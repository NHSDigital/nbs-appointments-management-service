using FluentAssertions;

namespace CosmosDbSeederTests;

public class GlobalRolesTests : BaseCosmosDbSeederTest
{
    [Fact]
    public void GlobalRolesShouldBeTheSameInEachEnvironment()
    {
        var localGlobalRoles = ReadGlobalRoles("local");
        var devGlobalRoles = ReadGlobalRoles("dev");
        var intGlobalRoles = ReadGlobalRoles("int");
        var perfGlobalRoles = ReadGlobalRoles("perf");
        var stagGlobalRoles = ReadGlobalRoles("stag");
        var prodGlobalRoles = ReadGlobalRoles("prod");

        devGlobalRoles.Should().BeEquivalentTo(intGlobalRoles);
        intGlobalRoles.Should().BeEquivalentTo(perfGlobalRoles);
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
