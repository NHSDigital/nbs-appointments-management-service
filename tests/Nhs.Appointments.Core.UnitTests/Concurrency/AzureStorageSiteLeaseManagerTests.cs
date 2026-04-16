using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Blob;
using Nhs.Appointments.Core.Concurrency;

namespace Nhs.Appointments.Core.UnitTests.Concurrency;

public class AzureStorageSiteLeaseManagerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void CannotConstructWithInvalidAcquireTime(int acquireTimeInSeconds)
    {
        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0) };
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        options.Setup(o => o.Value).Returns(slmo);

        var blobClient = new TestableBlobClient();
        var abs = new Mock<IAzureBlobStorage>();

        Action action = () => new AzureStorageSiteLeaseManager(options.Object, abs.Object, acquireTimeInSeconds);

        // Assert.
        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("acquireTimeInSeconds * be greater than '0'*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void CannotConstructWithInvalidDelayTime(int delayRetryTimeInMilliseconds)
    {
        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0) };
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        options.Setup(o => o.Value).Returns(slmo);

        var blobClient = new TestableBlobClient();
        var abs = new Mock<IAzureBlobStorage>();

        Action action = () => new AzureStorageSiteLeaseManager(options.Object, abs.Object, 10, delayRetryTimeInMilliseconds);

        // Assert.
        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("delayRetryTimeInMilliseconds * be greater than '0'*");
    }

    [Fact]
    public void SiteAndDateSupplied_Acquire_AcquiresLeaseContextWithExpectedKey()
    {
        // Arrange.
        var siteId = Guid.NewGuid().ToString();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var expectedSiteKey = LeaseKeys.SiteKeyFactory.Create(siteId, date);

        var containerName = Guid.NewGuid().ToString();

        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0), ContainerName = containerName };
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        options.Setup(o => o.Value).Returns(slmo);

        var blobClient = new TestableBlobClient();
        var abs = new Mock<IAzureBlobStorage>();
        abs.Setup(a => a.GetBlobClientFromContainerAndBlobName(containerName, expectedSiteKey)).Returns(blobClient);

        var sut = new AzureStorageSiteLeaseManager(options.Object, abs.Object);

        // Act.
        var slc = sut.Acquire(siteId, date);

        // Assert.
        slc.SiteKey.Should().Be(expectedSiteKey);
        abs.Verify();
        blobClient.RetainedBlobLeaseClient.Verify(blc => blc.Acquire(It.IsAny<TimeSpan>(), It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void TwoLocksAttemptedForSameSiteAndDifferentDate_Acquire_AcquiresSeparateLeaseContextsWithExpectedKeys()
    {
        // Arrange.
        var siteId = Guid.NewGuid().ToString();
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow);
        var date2 = date1.AddDays(1);
        var expectedSiteKey1 = LeaseKeys.SiteKeyFactory.Create(siteId, date1);
        var expectedSiteKey2 = LeaseKeys.SiteKeyFactory.Create(siteId, date2);

        var containerName = Guid.NewGuid().ToString();

        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0), ContainerName = containerName };
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        options.Setup(o => o.Value).Returns(slmo);

        var blobClient1 = new TestableBlobClient();
        var blobClient2 = new TestableBlobClient();
        var abs = new Mock<IAzureBlobStorage>();
        abs.Setup(a => a.GetBlobClientFromContainerAndBlobName(containerName, expectedSiteKey1)).Returns(blobClient1);
        abs.Setup(a => a.GetBlobClientFromContainerAndBlobName(containerName, expectedSiteKey2)).Returns(blobClient2);

        var sut = new AzureStorageSiteLeaseManager(options.Object, abs.Object);

        // Act.
        var slc1 = sut.Acquire(siteId, date1);
        var slc2 = sut.Acquire(siteId, date2);

        // Assert.
        slc1.SiteKey.Should().Be(expectedSiteKey1);
        slc2.SiteKey.Should().Be(expectedSiteKey2);
        abs.Verify();
        blobClient1.RetainedBlobLeaseClient.Verify(blc => blc.Acquire(It.IsAny<TimeSpan>(), It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()), Times.Once());
        blobClient2.RetainedBlobLeaseClient.Verify(blc => blc.Acquire(It.IsAny<TimeSpan>(), It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void TwoLocksAttemptedForDifferentSitesAndSameDate_Acquire_AcquiresSeparateLeaseContextsWithExpectedKeys()
    {
        // Arrange.
        var siteId1 = Guid.NewGuid().ToString();
        var siteId2 = Guid.NewGuid().ToString();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var expectedSiteKey1 = LeaseKeys.SiteKeyFactory.Create(siteId1, date);
        var expectedSiteKey2 = LeaseKeys.SiteKeyFactory.Create(siteId2, date);

        var containerName = Guid.NewGuid().ToString();

        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0), ContainerName = containerName };
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        options.Setup(o => o.Value).Returns(slmo);

        var blobClient1 = new TestableBlobClient();
        var blobClient2 = new TestableBlobClient();
        var abs = new Mock<IAzureBlobStorage>();
        abs.Setup(a => a.GetBlobClientFromContainerAndBlobName(containerName, expectedSiteKey1)).Returns(blobClient1);
        abs.Setup(a => a.GetBlobClientFromContainerAndBlobName(containerName, expectedSiteKey2)).Returns(blobClient2);

        var sut = new AzureStorageSiteLeaseManager(options.Object, abs.Object);

        // Act.
        var slc1 = sut.Acquire(siteId1, date);
        var slc2 = sut.Acquire(siteId2, date);

        // Assert.
        slc1.SiteKey.Should().Be(expectedSiteKey1);
        slc2.SiteKey.Should().Be(expectedSiteKey2);
        abs.Verify();
        blobClient1.RetainedBlobLeaseClient.Verify(blc => blc.Acquire(It.IsAny<TimeSpan>(), It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()), Times.Once());
        blobClient2.RetainedBlobLeaseClient.Verify(blc => blc.Acquire(It.IsAny<TimeSpan>(), It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void TwoLocksAttemptedForSameSiteAndSameDate_Acquire_AttemptToAcquireSameLeaseContextsWithExpectedKey()
    {
        // Arrange.
        var siteId = Guid.NewGuid().ToString();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var expectedSiteKey = LeaseKeys.SiteKeyFactory.Create(siteId, date);

        var containerName = Guid.NewGuid().ToString();

        var slmo = new SiteLeaseManagerOptions { Timeout = new TimeSpan(1, 0, 0), ContainerName = containerName };
        var options = new Mock<IOptions<SiteLeaseManagerOptions>>();
        options.Setup(o => o.Value).Returns(slmo);

        var blobClient = new TestableBlobClient();
        var abs = new Mock<IAzureBlobStorage>();
        abs.Setup(a => a.GetBlobClientFromContainerAndBlobName(containerName, expectedSiteKey)).Returns(blobClient);

        var sut = new AzureStorageSiteLeaseManager(options.Object, abs.Object);

        // Act.
        var slc1 = sut.Acquire(siteId, date);
        var slc2 = sut.Acquire(siteId, date);

        // Assert.
        slc1.SiteKey.Should().Be(expectedSiteKey);
        slc2.SiteKey.Should().Be(expectedSiteKey);
        abs.Verify();
        blobClient.RetainedBlobLeaseClient.Verify(blc => blc.Acquire(It.IsAny<TimeSpan>(), It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private class TestableBlobClient : BlobClient
    {
        public Mock<BlobLeaseClient> RetainedBlobLeaseClient { get; private set; }

        protected override BlobLeaseClient GetBlobLeaseClientCore(string leaseId)
        {
            if (RetainedBlobLeaseClient is null)
            {
                RetainedBlobLeaseClient = new Mock<BlobLeaseClient>();
            }

            return RetainedBlobLeaseClient.Object;
        }

        public override Response<bool> Exists(CancellationToken cancellationToken = default) 
        {
            var response = new Mock<Azure.Response<bool>>();
            response.Setup(r => r.Value).Returns(true);

            return response.Object;
        }
    }
}
