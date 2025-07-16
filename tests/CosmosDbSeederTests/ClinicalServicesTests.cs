using FluentAssertions;

namespace CosmosDbSeederTests;

public class ClinicalServicesTests : BaseCosmosDbSeederTest
{
    [Fact]
    public void ClinicalServicesShouldBeTheSameInEachEnvironment()
    {
        var localClinicalServices = ReadDocument<ClinicalServicesDocument>("local");
        var devClinicalServices = ReadDocument<ClinicalServicesDocument>("dev");
        var intClinicalServices = ReadDocument<ClinicalServicesDocument>("int");
        var penClinicalService = ReadDocument<ClinicalServicesDocument>("pen");
        var perfClinicalService = ReadDocument<ClinicalServicesDocument>("perf");
        var stagClinicalServices = ReadDocument<ClinicalServicesDocument>("stag");
        var prodClinicalServices = ReadDocument<ClinicalServicesDocument>("prod");

        localClinicalServices.Should().BeEquivalentTo(devClinicalServices);
        devClinicalServices.Should().BeEquivalentTo(intClinicalServices);
        intClinicalServices.Should().BeEquivalentTo(penClinicalService);
        penClinicalService.Should().BeEquivalentTo(perfClinicalService);
        perfClinicalService.Should().BeEquivalentTo(stagClinicalServices);
        stagClinicalServices.Should().BeEquivalentTo(prodClinicalServices);
    }

    [Fact(DisplayName = "APPT-740: Define Autumn/Winter Clinical Services")]
    public void ClinicalRolesShouldBeCorrect()
    {
        var clinicalServices = ReadDocument<ClinicalServicesDocument>("local").Services;

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
