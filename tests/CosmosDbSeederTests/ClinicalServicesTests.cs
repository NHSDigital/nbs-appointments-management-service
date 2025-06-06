using FluentAssertions;
using Newtonsoft.Json;

namespace CosmosDbSeederTests;

public class ClinicalServicesTests
{
    private static ClinicalServicesDocument ReadClinicalServices(string environment)
    {
        var ClinicalServices = JsonConvert.DeserializeObject<ClinicalServicesDocument>(File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            $"items/{environment}/core_data/clinical_services.json")));

        return ClinicalServices ??
               throw new Exception($"Could not read clinical_services.json from {environment} environment");
    }

    [Fact]
    public void ClinicalServicesShouldBeTheSameInEachEnvironment()
    {
        var localClinicalServices = ReadClinicalServices("local");
        var devClinicalServices = ReadClinicalServices("dev");
        var intClinicalServices = ReadClinicalServices("int");
        var stagClinicalServices = ReadClinicalServices("stag");
        var prodClinicalServices = ReadClinicalServices("prod");

        localClinicalServices.Should().BeEquivalentTo(devClinicalServices);
        devClinicalServices.Should().BeEquivalentTo(intClinicalServices);
        intClinicalServices.Should().BeEquivalentTo(stagClinicalServices);
        stagClinicalServices.Should().BeEquivalentTo(prodClinicalServices);
    }

    [Fact(DisplayName = "APPT-740: Define Autumn/Winter Clinical Services")]
    public void ClinicalRolesShouldBeCorrect()
    {
        var clinicalServices = ReadClinicalServices("local").Services;

        clinicalServices.Should().Contain(service => service.Id == "RSV:Adult" && service.Label == "RSV Adult");
        clinicalServices.Should().Contain(service => service.Id == "COVID:5_11" && service.Label == "COVID 5-11");
        clinicalServices.Should().Contain(service => service.Id == "COVID:12_17" && service.Label == "COVID 12-17");
        clinicalServices.Should().Contain(service => service.Id == "COVID:18+" && service.Label == "COVID 18+");
        clinicalServices.Should().Contain(service => service.Id == "FLU:18_64" && service.Label == "Flu 18-64");
        clinicalServices.Should().Contain(service => service.Id == "FLU:65+" && service.Label == "Flu 65+");
        clinicalServices.Should()
            .Contain(service => service.Id == "COVID_FLU:18_64" && service.Label == "Flu and COVID 18-64");
        clinicalServices.Should()
            .Contain(service => service.Id == "COVID_FLU:65+" && service.Label == "Flu and COVID 65+");
        clinicalServices.Should().Contain(service => service.Id == "FLU:2_3" && service.Label == "Flu 2-3");

        clinicalServices.Should().HaveCount(9);
    }
}
