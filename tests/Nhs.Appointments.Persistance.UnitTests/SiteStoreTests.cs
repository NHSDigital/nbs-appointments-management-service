using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Persistance.Models;
using System.Linq.Expressions;

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
                null, null,
                string.Empty));
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
                SiteStatus.Online, null,
                string.Empty));
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

    [Fact]
    public async Task ToggleSiteSoftDeletionAsync_UpdatesSiteSoftDeletionStatus()
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
                SiteStatus.Online, false,
                string.Empty));
        _siteStore.Setup(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()))
            .Callback<string, string, PatchOperation[]>((site, reference, patches)
                => patchOperations = [.. patches])
            .ReturnsAsync(null as SiteDocument);

        var result = await _sut.ToggleSiteSoftDeletionAsync("some-site-id");

        result.Success.Should().BeTrue();

        patchOperations.Should().HaveCount(1);
        patchOperations.First().OperationType.Should().Be(PatchOperationType.Replace);
        patchOperations.First().Path.Should().Be("/isDeleted");

        _siteStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Once);
    }

    [Fact]
    public async Task ToggleSiteSoftDeletionAsync_AddIsDeletedProperty()
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
                SiteStatus.Online, null,
                string.Empty));
        _siteStore.Setup(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()))
            .Callback<string, string, PatchOperation[]>((site, reference, patches)
                => patchOperations = [.. patches])
            .ReturnsAsync(null as SiteDocument);

        var result = await _sut.ToggleSiteSoftDeletionAsync("some-site-id");

        result.Success.Should().BeTrue();

        patchOperations.Should().HaveCount(1);
        patchOperations.First().OperationType.Should().Be(PatchOperationType.Add);
        patchOperations.First().Path.Should().Be("/isDeleted");

        _siteStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Once);
    }

    [Fact]
    public async Task ToggleSiteSoftDeletionAsync_FailsToFindSite()
    {
        _siteStore.Setup(x => x.GetDocument<Site>(It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var result = await _sut.ToggleSiteSoftDeletionAsync("some-site-id");

        result.Success.Should().BeFalse();

        _siteStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Never);
    }

    [Fact]
    public async Task GetAllSites_ConvertsAccessNeedsToLowercase()
    {
        _siteStore.Setup(x => x.RunQueryAsync<Site>(It.IsAny<Expression<Func<SiteDocument, bool>>>()))
            .ReturnsAsync(new[]
            {
                new Site(
                    "site-1",
                    "Site One",
                    "Address One",
                    "01234567890",
                    "ODS1",
                    "R1",
                    "ICB1",
                    "Type1",
                    new[]
                    {
                        new Accessibility("wheelchair_access", "TRUE"),
                        new Accessibility("induction_loop", "TrUe"),
                        new Accessibility("visual_aids", "trUE"),
                        new Accessibility("car_parking", "TruE")
                    },
                    new Location("Coordinates", [-1.75, 52.76]),
                    SiteStatus.Online, null,
                    string.Empty),
                new Site(
                    "site-2",
                    "Site Two",
                    "Address Two",
                    "09876543210",
                    "ODS2",
                    "R2",
                    "ICB2",
                    "Type2",
                    new[]
                    {
                        new Accessibility("wheelchair_access", "false"),
                        new Accessibility("induction_loop", "FALSE"),
                        new Accessibility("visual_aids", "fAlsE"),
                        new Accessibility("car_parking", "falSE")
                    },
                    new Location("Coordinates", [-2.75, 53.76]),
                    SiteStatus.Offline, null,
                    string.Empty)
            });

        var result = await _sut.GetAllSites();

        result.Count().Should().Be(2);
        result.First().Accessibilities.All(x => x.Value == "true").Should().BeTrue();
        result.Skip(1).First().Accessibilities.All(x => x.Value == "false").Should().BeTrue();
    }
}
