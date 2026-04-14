using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Concurrency;

namespace Nhs.Appointments.Core.UnitTests.Concurrency;

public class InMemorySiteLeaseManagerTests
{
    [Fact]
    public void SiteAndDateSupplied_Acquire_ReturnsSiteLeaseContext()
    {
        // Arrange.
        var siteId = Guid.NewGuid().ToString();
        var date  = DateOnly.FromDateTime(DateTime.UtcNow);
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0) };
        options.Setup(o => o.Value).Returns(slmo);

        var expectedSiteKey = LeaseKeys.SiteKeyFactory.Create(siteId, date);

        var sut = new InMemorySiteLeaseManager(options.Object);

        // Act.
        var slc = sut.Acquire(siteId, date);

        // Assert.
        slc.SiteKey.Should()
            .Be(expectedSiteKey);
    }
}
