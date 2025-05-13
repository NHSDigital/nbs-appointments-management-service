using FluentAssertions;
using Newtonsoft.Json;

namespace CosmosDbSeederTests;

public class GlobalRolesTests
{
    private static object ReadGlobalRoles(string environment)
    {
        var globalRoles = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(
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
    }
}
