using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.ClinicalServices;
using Nhs.Appointments.Core.OdsCodes;
using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.UnitTests.Reports;

public class SiteReportServiceTests
{
    private readonly SiteReportService _sut;

    private readonly Mock<IDailySiteSummaryStore> _dailySiteSummaryStore = new();
    private readonly Mock<IClinicalServiceStore> _clinicalServiceStore = new();
    private readonly Mock<IWellKnownOdsCodesStore> _wellKnownOdsCodesStore = new();
    private readonly Mock<IOptions<SiteSummaryQueryOptions>> _siteSummaryQueryOptions = new();

    public SiteReportServiceTests()
    {
        _clinicalServiceStore
            .Setup(store => store.Get())
            .ReturnsAsync(SiteReportServiceMockData.MockClinicalServices);

        _wellKnownOdsCodesStore
            .Setup(store => store.GetWellKnownOdsCodesDocument())
            .ReturnsAsync(SiteReportServiceMockData.MockWellKnownOdsCodes);

        _dailySiteSummaryStore.Setup(store => store.GetSiteSummaries(
                SiteReportServiceMockData.Site1Guid.ToString(),
                SiteReportServiceMockData.DayOne,
                SiteReportServiceMockData.DayThree))
            .ReturnsAsync(SiteReportServiceMockData.MockSite1DailySummaries);

        _dailySiteSummaryStore.Setup(store => store.GetSiteSummaries(
                SiteReportServiceMockData.Site2Guid.ToString(),
                SiteReportServiceMockData.DayOne,
                SiteReportServiceMockData.DayThree))
            .ReturnsAsync(SiteReportServiceMockData.MockSite2DailySummaries);

        _dailySiteSummaryStore.Setup(store => store.GetSiteSummaries(
                SiteReportServiceMockData.Site3Guid.ToString(),
                SiteReportServiceMockData.DayOne,
                SiteReportServiceMockData.DayThree))
            .ReturnsAsync(SiteReportServiceMockData.MockSite3DailySummaries);
        
        _siteSummaryQueryOptions.Setup(options => options.Value).Returns(new SiteSummaryQueryOptions
        {
            MinimumParallelization = 500
        });

        _sut = new SiteReportService(_dailySiteSummaryStore.Object,
            _clinicalServiceStore.Object, _wellKnownOdsCodesStore.Object,  _siteSummaryQueryOptions.Object);
    }

    [Fact]
    public async Task GeneratesAReport()
    {
        var reports = (await _sut.GenerateReports(SiteReportServiceMockData.MockSites, SiteReportServiceMockData.DayOne,
            SiteReportServiceMockData.DayThree)).ToList();

        reports.Count.Should().Be(3);

        _clinicalServiceStore.Verify(store => store.Get(), Times.Once);
        _wellKnownOdsCodesStore.Verify(store => store.GetWellKnownOdsCodesDocument(), Times.Once);

        _dailySiteSummaryStore.Verify(store => store.GetSiteSummaries(
                SiteReportServiceMockData.Site1Guid.ToString(),
                SiteReportServiceMockData.DayOne,
                SiteReportServiceMockData.DayThree),
            Times.Once);

        _dailySiteSummaryStore.Verify(store => store.GetSiteSummaries(
                SiteReportServiceMockData.Site2Guid.ToString(),
                SiteReportServiceMockData.DayOne,
                SiteReportServiceMockData.DayThree),
            Times.Once);

        _dailySiteSummaryStore.Verify(store => store.GetSiteSummaries(
                SiteReportServiceMockData.Site3Guid.ToString(),
                SiteReportServiceMockData.DayOne,
                SiteReportServiceMockData.DayThree),
            Times.Once);

        var site1Summary = reports.Single(item => item.SiteName == "Site 1");
        site1Summary.SiteType.Should().Be("GP Practice");
        site1Summary.Status.Should().Be("Online");
        site1Summary.ICB.Should().Be("ICB1");
        site1Summary.ICBName.Should().Be("Integrated Care Board One");
        site1Summary.Region.Should().Be("R1");
        site1Summary.RegionName.Should().Be("Region One");
        site1Summary.OdsCode.Should().Be("ABC01");
        site1Summary.Longitude.Should().Be(.505);
        site1Summary.Latitude.Should().Be(65);
        site1Summary.Bookings["RSV:Adult"].Should().Be(35 + 38 + 29 + 7 + 8 + 2);
        site1Summary.Bookings["COVID:5_11"].Should().Be(78 + 67 + 96 + 23 + 15 + 22);
        site1Summary.TotalBookings.Should().Be(35 + 38 + 29 + 7 + 8 + 2 + 78 + 67 + 96 + 23 + 15 + 22);
        site1Summary.Cancelled.Should().Be(29 + 22 + 35);
        site1Summary.RemainingCapacity["RSV:Adult"].Should().Be(65 + 62 + 71);
        site1Summary.RemainingCapacity["COVID:5_11"].Should().Be(22 + 33 + 4);
        site1Summary.MaximumCapacity.Should().Be(200 + 200 + 200);

        var site2Summary = reports.Single(item => item.SiteName == "Site 2");
        site2Summary.SiteType.Should().Be("GP Practice");
        site2Summary.Status.Should().Be("Offline");
        site2Summary.ICB.Should().Be("ICB1");
        site2Summary.ICBName.Should().Be("Integrated Care Board One");
        site2Summary.Region.Should().Be("R1");
        site2Summary.RegionName.Should().Be("Region One");
        site2Summary.OdsCode.Should().Be("ABC02");
        site2Summary.Longitude.Should().Be(.506);
        site2Summary.Latitude.Should().Be(65);
        site2Summary.Bookings["RSV:Adult"].Should().Be(42 + 45 + 52 + 5 + 6 + 9);
        site2Summary.Bookings["COVID:5_11"].Should().Be(58 + 85 + 73 + 12 + 20 + 17);
        site2Summary.TotalBookings.Should().Be(42 + 45 + 52 + 5 + 6 + 9 + 58 + 85 + 73 + 12 + 20 + 17);
        site2Summary.Cancelled.Should().Be(15 + 18 + 25);
        site2Summary.RemainingCapacity["RSV:Adult"].Should().Be(58 + 55 + 48);
        site2Summary.RemainingCapacity["COVID:5_11"].Should().Be(42 + 15 + 27);
        site2Summary.MaximumCapacity.Should().Be(200 + 200 + 200);

        var site3Summary = reports.Single(item => item.SiteName == "Site 3");
        site3Summary.SiteType.Should().Be("GP Practice");
        site3Summary.Status.Should().Be("Online");
        site3Summary.ICB.Should().Be("ICB1");
        site3Summary.ICBName.Should().Be("Integrated Care Board One");
        site3Summary.Region.Should().Be("R1");
        site3Summary.RegionName.Should().Be("Region One");
        site3Summary.OdsCode.Should().Be("ABC03");
        site3Summary.Longitude.Should().Be(.507);
        site3Summary.Latitude.Should().Be(65);
        site3Summary.Bookings["RSV:Adult"].Should().Be(28 + 31 + 47 + 3 + 4 + 6);
        site3Summary.Bookings["COVID:5_11"].Should().Be(92 + 74 + 68 + 18 + 16 + 14);
        site3Summary.TotalBookings.Should().Be(28 + 31 + 47 + 3 + 4 + 6 + 92 + 74 + 68 + 18 + 16 + 14);
        site3Summary.Cancelled.Should().Be(33 + 27 + 31);
        site3Summary.RemainingCapacity["RSV:Adult"].Should().Be(72 + 69 + 53);
        site3Summary.RemainingCapacity["COVID:5_11"].Should().Be(8 + 26 + 32);
        site3Summary.MaximumCapacity.Should().Be(200 + 200 + 200);
    }
}
