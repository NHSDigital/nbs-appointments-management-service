using CapacityDataExtracts.Documents;
using FluentAssertions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace CapacityDataExtracts.UnitTests;
public class CapacityDataConverterTests
{
    [Fact]
    public void ExtractSessionExtractDate_GetsCorrectData()
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00));
        var result = CapacityDataConverter.ExtractDate(testDocument);
        result.Should().Be("2025-01-02");
    }

    [Fact]
    public void ExtractSessionExtractTime_GetsCorrectData()
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00));
        var result = CapacityDataConverter.ExtractTime(testDocument);
        result.Should().Be("09:00");
    }

    [Fact]
    public void ExtractSessionExtractSlotLength_GetsCorrectData()
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00));
        var result = CapacityDataConverter.ExtractSlotLength(testDocument);
        result.Should().Be("05");
    }

    [Theory]
    [InlineData("1", "ODS_01")]
    [InlineData("2", "ODS_02")]
    public void ExtractSessionExtractOdsCode_GetsCorrectData(string siteId, string expected)
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Site = siteId,
        };
        var converter = new CapacityDataConverter(TestSites);
        var result = converter.ExtractOdsCode(testDocument);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", 0.1)]
    [InlineData("2", 0.2)]
    public void ExtractSessionExtractLatitude_GetsCorrectData(string siteId, double expected)
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Site = siteId,
        };
        var converter = new CapacityDataConverter(TestSites);
        var result = converter.ExtractLatitude(testDocument);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", 1.0)]
    [InlineData("2", 2.0)]
    public void ExtractSessionExtractLongitude_GetsCorrectData(string siteId, double expected)
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Site = siteId,
        };
        var converter = new CapacityDataConverter(TestSites);
        var result = converter.ExtractLongitude(testDocument);
        result.Should().Be(expected);
    }

    [Fact]
    public void ExtractSessionExtractCapacity_GetsCorrectData()
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Capacity = 5
        };
        var result = CapacityDataConverter.ExtractCapacity(testDocument);
        result.Should().Be(5);
    }

    [Theory]
    [InlineData("1", "Site One")]
    [InlineData("2", "Site Two")]
    public void ExtractSessionExtractSiteName_GetsCorrectData(string siteId, string expected)
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Site = siteId,
        };
        var converter = new CapacityDataConverter(TestSites);
        var result = converter.ExtractSiteName(testDocument);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", "RGN01")]
    [InlineData("2", "RGN02")]
    public void ExtractSessionExtractRegion_GetsCorrectData(string siteId, string expected)
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Site = siteId,
        };
        var converter = new CapacityDataConverter(TestSites);
        var result = converter.ExtractRegion(testDocument);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", "ICB01")]
    [InlineData("2", "ICB02")]
    public void ExtractSessionExtractICB_GetsCorrectData(string siteId, string expected)
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Site = siteId,
        };
        var converter = new CapacityDataConverter(TestSites);
        var result = converter.ExtractICB(testDocument);
        result.Should().Be(expected);
    }

    [Fact]
    public void ExtractSessionExtractService_GetsCorrectData()
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Services = ["service1"]
        };
        var result = CapacityDataConverter.ExtractService(testDocument);
        result[0].Should().Be("service1");
    }

    [Fact]
    public void ExtractSessionExtractService_Multiple_GetsCorrectData()
    {
        var testDocument = new SiteSessionInstance("", new DateTime(2025, 01, 02, 9, 00, 00), new DateTime(2025, 01, 02, 9, 05, 00))
        {
            Services = ["service1", "service2"]
        };
        var result = CapacityDataConverter.ExtractService(testDocument);
        result[0].Should().Be("service1");
        result[1].Should().Be("service2");
    }

    private IEnumerable<SiteDocument> TestSites => new[]
{
        new SiteDocument
        {
            Id = "1",
            Name = "Site One",
            OdsCode = "ODS_01",
            IntegratedCareBoard = "ICB01",
            Region = "RGN01",
            Location = new Location("Point", new []{1.0, 0.1 })
        },
        new SiteDocument
        {
            Id = "2",
            Name = "Site Two",
            OdsCode = "ODS_02",
            IntegratedCareBoard = "ICB02",
            Region = "RGN02",
            Location = new Location("Point", new []{2.0, 0.2 })
        }
    };
}
