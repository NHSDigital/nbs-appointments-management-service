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

    [Fact]
    public void SameSiteAndSameDayCallsConcurrent_Acquire_AttemptToAcquireTheSameLock()
    {
        // Arrange.
        var siteId = Guid.NewGuid().ToString();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(0, 0, 0, 0, 0, 1) };
        options.Setup(o => o.Value).Returns(slmo);

        var expectedSiteKey = LeaseKeys.SiteKeyFactory.Create(siteId, date);

        var sut = new InMemorySiteLeaseManager(options.Object);

        // Act.
        var slc1 = sut.Acquire(siteId, date);
        Action action = () => sut.Acquire(siteId, date);

        // Assert.
        slc1.SiteKey.Should()
            .Be(expectedSiteKey);
        action.Should()
            .Throw<AbandonedMutexException>()
            .WithMessage($"Abandoned attempt to acquire lock for site key {expectedSiteKey}");
    }

    [Fact]
    public void SameSiteAndSameDayCallsButNotConcurrent_Acquire_ReturnsMatchingSiteLeaseContexts()
    {
        // Arrange.
        var siteId = Guid.NewGuid().ToString();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(0, 0, 0, 0, 0, 1) };
        options.Setup(o => o.Value).Returns(slmo);

        var expectedSiteKey = LeaseKeys.SiteKeyFactory.Create(siteId, date);

        var sut = new InMemorySiteLeaseManager(options.Object);

        // Act.
        var originalSiteKey = "";
        using (var slc1 = sut.Acquire(siteId, date))
        {
            originalSiteKey = slc1.SiteKey;
        }
        var slc2 = sut.Acquire(siteId, date);

        // Assert.
        originalSiteKey.Should()
            .Be(expectedSiteKey);
        slc2.SiteKey.Should()
            .Be(expectedSiteKey);
    }

    [Fact]
    public void SameSiteAndDifferentDayCalls_Acquire_ReturnsSeparateSiteLeaseContexts()
    {
        // Arrange.
        var siteId = Guid.NewGuid().ToString();
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow);
        var date2 = date1.AddDays(1);
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0) };
        options.Setup(o => o.Value).Returns(slmo);

        var expectedSiteKey1 = LeaseKeys.SiteKeyFactory.Create(siteId, date1);
        var expectedSiteKey2 = LeaseKeys.SiteKeyFactory.Create(siteId, date2);

        var sut = new InMemorySiteLeaseManager(options.Object);

        // Act.
        var slc1 = sut.Acquire(siteId, date1);
        var slc2 = sut.Acquire(siteId, date2);

        // Assert.
        slc1.SiteKey.Should()
            .Be(expectedSiteKey1);
        slc2.SiteKey.Should()
            .Be(expectedSiteKey2);
        expectedSiteKey1.Should().NotBe(expectedSiteKey2);
    }

    [Fact]
    public void DifferentSiteAndSameDayCalls_Acquire_ReturnsSeparateSiteLeaseContexts()
    {
        // Arrange.
        var siteId1 = Guid.NewGuid().ToString();
        var siteId2 = Guid.NewGuid().ToString();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0) };
        options.Setup(o => o.Value).Returns(slmo);

        var expectedSiteKey1 = LeaseKeys.SiteKeyFactory.Create(siteId1, date);
        var expectedSiteKey2 = LeaseKeys.SiteKeyFactory.Create(siteId2, date);

        var sut = new InMemorySiteLeaseManager(options.Object);

        // Act.
        var slc1 = sut.Acquire(siteId1, date);
        var slc2 = sut.Acquire(siteId2, date);

        // Assert.
        slc1.SiteKey.Should()
            .Be(expectedSiteKey1);
        slc2.SiteKey.Should()
            .Be(expectedSiteKey2);
        expectedSiteKey1.Should().NotBe(expectedSiteKey2);
    }
}
