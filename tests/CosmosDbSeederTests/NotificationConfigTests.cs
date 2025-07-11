using FluentAssertions;

namespace CosmosDbSeederTests;

public class NotificationConfigTests : BaseCosmosDbSeederTest
{
    [Theory]
    [InlineData("local")]
    [InlineData("dev")]
    [InlineData("int")]
    [InlineData("pen")]
    [InlineData("stag")]
    [InlineData("prod")]
    public void AllConfigsAreForValidServices(string environment)
    {
        var notificationConfigs = ReadDocument<NotificationConfigDocument>(environment);
        var clinicalServiceIds =
            ReadDocument<ClinicalServicesDocument>(environment).Services.Select(s => s.Id).ToArray();

        foreach (var config in notificationConfigs.Configs)
        {
            foreach (var serviceId in config.Services)
            {
                clinicalServiceIds.Should().Contain(serviceId);
            }
        }
    }
}
