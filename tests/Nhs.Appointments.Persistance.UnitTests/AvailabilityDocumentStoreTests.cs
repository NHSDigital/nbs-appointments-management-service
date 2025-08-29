using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests;
public class AvailabilityDocumentStoreTests
{
    private readonly Mock<ITypedDocumentCosmosStore<DailyAvailabilityDocument>> _documentStore = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    private readonly AvailabilityDocumentStore _sut;

    public AvailabilityDocumentStoreTests()
    {
        _sut = new AvailabilityDocumentStore(_documentStore.Object, _metricsRecorder.Object);
    }

    [Fact]
    public async Task CancelDayAsync_ThrowsException_WhenAvailabilityNotFoundAtSite()
    {
        _documentStore.Setup(x => x.GetByIdOrDefaultAsync<DailyAvailabilityDocument>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(null as DailyAvailabilityDocument);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.CancelDayAsync("TEST_SITE_123", new DateOnly(2025, 1, 1)));
    }

    [Fact]
    public async Task CancelDayAsync_RemovesSessionsForTheDay()
    {
        var date = new DateOnly(2025, 1, 1);
        _documentStore.Setup(x => x.GetByIdOrDefaultAsync<DailyAvailabilityDocument>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DailyAvailabilityDocument
            {
                Date = date,
                DocumentType = "availability",
                Id =  "20250101",
                Site = "TEST_SITE_123",
                Sessions = [
                    new()
                    {
                        Capacity = 2,
                        From = new TimeOnly(12, 00),
                        Services = ["RSV:Adult"],
                        Until = new TimeOnly(18, 00),
                        SlotLength = 5
                    }
                ]
            });

        await _sut.CancelDayAsync("TEST_SITE_123", date);

        _documentStore.Verify(x => x.PatchDocument("TEST_SITE_123", "20250101", It.IsAny<PatchOperation[]>()), Times.Once);
    }
}
