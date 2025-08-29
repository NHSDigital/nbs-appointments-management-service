using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests;
public class SiteStoreTests
{
    private readonly Mock<ITypedDocumentCosmosStore<SiteDocument>> _siteStore = new();

    private readonly SiteStore _sut;

    public SiteStoreTests()
    {
        _sut = new SiteStore(_siteStore.Object);
    }

    [Fact]
    public async Task UpdateSiteStatusAsync_ReturnsUnsuccessful_WhenSiteDoesNotExist()
    {
        _siteStore.Setup(x => x.GetDocument<Site>(It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var result = await _sut.UpdateSiteStatusAsync("some-site-id", SiteStatus.Online);

        result.Success.Should().BeFalse();

        _siteStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSiteStatusAsync_AddsStatusPropertyToDocument()
    {
        List<PatchOperation> patchOperations = [];

        _siteStore.Setup(x => x.GetDocument<Site>(It.IsAny<string>()))
            .ReturnsAsync(new Site(
                "some-site-id",
                "Test Site",
                "Test Address",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                "Information",
                [],
                new Location("Coordinates", [-1.75, 52.76]),
                null));
        _siteStore.Setup(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()))
            .Callback<string, string, PatchOperation[]>((site, reference, patches)
                => patchOperations = [.. patches])
            .ReturnsAsync(null as SiteDocument);

        var result = await _sut.UpdateSiteStatusAsync("some-site-id", SiteStatus.Offline);

        result.Success.Should().BeTrue();

        patchOperations.Should().HaveCount(1);
        patchOperations.First().OperationType.Should().Be(PatchOperationType.Add);
        patchOperations.First().Path.Should().Be("/status");

        _siteStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSiteStatusAsync_UpdatesStatusPropertyOnDocument()
    {
        List<PatchOperation> patchOperations = [];

        _siteStore.Setup(x => x.GetDocument<Site>(It.IsAny<string>()))
            .ReturnsAsync(new Site(
                "some-site-id",
                "Test Site",
                "Test Address",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                "Information",
                [],
                new Location("Coordinates", [-1.75, 52.76]),
                SiteStatus.Online));
        _siteStore.Setup(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()))
            .Callback<string, string, PatchOperation[]>((site, reference, patches)
                => patchOperations = [.. patches])
            .ReturnsAsync(null as SiteDocument);

        var result = await _sut.UpdateSiteStatusAsync("some-site-id", SiteStatus.Offline);

        result.Success.Should().BeTrue();

        patchOperations.Should().HaveCount(1);
        patchOperations.First().OperationType.Should().Be(PatchOperationType.Replace);
        patchOperations.First().Path.Should().Be("/status");

        _siteStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Once);
    }
}
