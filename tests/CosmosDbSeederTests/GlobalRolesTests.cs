using FluentAssertions;
using Newtonsoft.Json;

namespace CosmosDbSeederTests;

public class GlobalRolesTests
{
    private static GlobalRolesDocument ReadGlobalRoles(string environment)
    {
        var globalRoles = JsonConvert.DeserializeObject<GlobalRolesDocument>(File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            $"items/{environment}/core_data/global_roles.json")));

        return globalRoles ?? throw new Exception($"Could not read global_roles.json from {environment} environment");
    }

    [Fact]
    public void GlobalRolesShouldBeTheSameInEachEnvironment()
    {
        var localGlobalRoles = ReadGlobalRoles("local");
        var devGlobalRoles = ReadGlobalRoles("dev");
        var intGlobalRoles = ReadGlobalRoles("int");
        var stagGlobalRoles = ReadGlobalRoles("stag");
        var prodGlobalRoles = ReadGlobalRoles("prod");

        devGlobalRoles.Should().BeEquivalentTo(intGlobalRoles);
        intGlobalRoles.Should().BeEquivalentTo(stagGlobalRoles);
        stagGlobalRoles.Should().BeEquivalentTo(prodGlobalRoles);

        localGlobalRoles.Roles =
            localGlobalRoles.Roles.Where(role => role.Id != "system:integration-test-user").ToArray();
        localGlobalRoles.Should().BeEquivalentTo(devGlobalRoles);
    }

    [Fact(DisplayName = "APPT-802: Admin User roles should include user manager")]
    public void AdminUserRolesShouldIncludeManageUsers()
    {
        var adminRoles = ReadGlobalRoles("local").Roles.Single(role => role.Id == "system:admin-user");

        adminRoles.Permissions.Should().Contain("users:manage");
    }
}
