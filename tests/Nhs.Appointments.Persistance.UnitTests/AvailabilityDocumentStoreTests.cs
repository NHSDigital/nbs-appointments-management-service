using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Moq;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    [Fact]
    public async Task CancelMultipleSessions_ReturnsFailure_WhenNoDocumentsFound()
    {
        var from = new DateOnly(2025, 10, 10);
        var until = new DateOnly(2025, 10, 12);
        var site = "TEST123";
        _documentStore.Setup(x => x.RunQueryAsync<DailyAvailabilityDocument>(It.IsAny<Expression<Func<DailyAvailabilityDocument, bool>>>()))
            .ReturnsAsync((IEnumerable<DailyAvailabilityDocument>)null);

        var result = await _sut.CancelMultipleSessions(site, from, until);

        result.Success.Should().BeFalse();
        result.Message.Should().Be($"No matching documents found for date range From: {from} - Until: {until} for Site: {site}");

        _documentStore.Verify(x => x.PatchDocument("TEST123", "20251010", It.IsAny<PatchOperation[]>()), Times.Never);
    }

    [Fact]
    public async Task CancelMultipleSessions_CancelsAllSessions_ForSingleDay()
    {
        _documentStore.Setup(x => x.RunQueryAsync<DailyAvailabilityDocument>(It.IsAny<Expression<Func<DailyAvailabilityDocument, bool>>>()))
            .ReturnsAsync(new List<DailyAvailabilityDocument>
            {
                new()
                {
                    Date = new DateOnly(2025, 10, 10),
                    DocumentType = "availability",
                    Id =  "20250101",
                    Site = "TEST123",
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
                }
            });

        var result = await _sut.CancelMultipleSessions("TEST123", new DateOnly(2025, 10, 10), new DateOnly(2025, 10, 10));

        result.Success.Should().BeTrue();

        _documentStore.Verify(x => x.PatchDocument("TEST123", "20251010", It.IsAny<PatchOperation[]>()), Times.Once);
    }

    [Fact]
    public async Task CancelMultipleSessions_CancelsAllSessions_ForMultipleDays()
    {
        _documentStore.Setup(x => x.RunQueryAsync<DailyAvailabilityDocument>(It.IsAny<Expression<Func<DailyAvailabilityDocument, bool>>>()))
            .ReturnsAsync(new List<DailyAvailabilityDocument>
            {
                new()
                {
                    Date = new DateOnly(2025, 10, 10),
                    DocumentType = "availability",
                    Id =  "20250101",
                    Site = "TEST123",
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
                },
                new()
                {
                    Date = new DateOnly(2025, 10, 11),
                    DocumentType = "availability",
                    Id =  "20250101",
                    Site = "TEST123",
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
                },
                new()
                {
                    Date = new DateOnly(2025, 10, 12),
                    DocumentType = "availability",
                    Id =  "20250101",
                    Site = "TEST123",
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
                }
            });

        var result = await _sut.CancelMultipleSessions("TEST123", new DateOnly(2025, 10, 10), new DateOnly(2025, 10, 10));

        result.Success.Should().BeTrue();

        _documentStore.Verify(x => x.PatchDocument("TEST123", It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Exactly(3));
    }

    [Fact]
    public async Task EditSessions_ReturnsFailure_WhenNoDocumentsFound()
    {
        var from = new DateOnly(2025, 10, 10);
        var until = new DateOnly(2025, 10, 12);
        var site = "TEST123";
        var sessionMatcher = new Session
        {
            Capacity = 2,
            From = new TimeOnly(12, 00),
            Services = ["RSV:Adult"],
            Until = new TimeOnly(18, 00),
            SlotLength = 5
        };
        var sessionReplacement = new Session
        {
            Capacity = 2,
            From = new TimeOnly(15, 00),
            Services = ["RSV:Adult", "COVID:65+"],
            Until = new TimeOnly(17, 00),
            SlotLength = 5
        };

        _documentStore.Setup(x => x.RunQueryAsync<DailyAvailabilityDocument>(It.IsAny<Expression<Func<DailyAvailabilityDocument, bool>>>()))
            .ReturnsAsync((IEnumerable<DailyAvailabilityDocument>)null);

        var result = await _sut.EditSessionsAsync(site, from, until, sessionMatcher,sessionReplacement);

        result.Success.Should().BeFalse();
        result.Message.Should().Be($"No matching documents found for date range From: {from} - Until: {until} for Site: {site}");

        _documentStore.Verify(x => x.PatchDocument("TEST123", It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Never);
    }

    [Fact]
    public async Task EditSessions_ReturnsSuccess_ForSingleDay()
    {
        var from = new DateOnly(2025, 10, 10);
        var until = new DateOnly(2025, 10, 10);
        var site = "TEST123";
        var sessionMatcher = new Session
        {
            Capacity = 2,
            From = new TimeOnly(12, 00),
            Services = ["RSV:Adult"],
            Until = new TimeOnly(18, 00),
            SlotLength = 5
        };
        var sessionReplacement = new Session
        {
            Capacity = 2,
            From = new TimeOnly(15, 00),
            Services = ["RSV:Adult", "COVID:65+"],
            Until = new TimeOnly(17, 00),
            SlotLength = 5
        };

        _documentStore.Setup(x => x.RunQueryAsync<DailyAvailabilityDocument>(It.IsAny<Expression<Func<DailyAvailabilityDocument, bool>>>()))
            .ReturnsAsync(new List<DailyAvailabilityDocument>
            {
                new()
                {
                    Date = new DateOnly(2025, 10, 10),
                    DocumentType = "availability",
                    Id =  "20251010",
                    Site = "TEST123",
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
                }
            });

        var result = await _sut.EditSessionsAsync(site, from, until, sessionMatcher, sessionReplacement);

        result.Success.Should().BeTrue();

        _documentStore.Verify(x => x.PatchDocument(site, "20251010", It.IsAny<PatchOperation[]>()), Times.Once);
    }

    [Fact]
    public async Task EditSessions_ReturnsSuccess_ForMultipleDays()
    {
        var from = new DateOnly(2025, 10, 10);
        var until = new DateOnly(2025, 10, 12);
        var site = "TEST123";
        var sessionMatcher = new Session
        {
            Capacity = 2,
            From = new TimeOnly(12, 00),
            Services = ["RSV:Adult"],
            Until = new TimeOnly(18, 00),
            SlotLength = 5
        };
        var sessionReplacement = new Session
        {
            Capacity = 2,
            From = new TimeOnly(15, 00),
            Services = ["RSV:Adult", "COVID:65+"],
            Until = new TimeOnly(17, 00),
            SlotLength = 5
        };

        _documentStore.Setup(x => x.RunQueryAsync<DailyAvailabilityDocument>(It.IsAny<Expression<Func<DailyAvailabilityDocument, bool>>>()))
            .ReturnsAsync(new List<DailyAvailabilityDocument>
            {
                new()
                {
                    Date = new DateOnly(2025, 10, 10),
                    DocumentType = "availability",
                    Id =  "20251010",
                    Site = "TEST123",
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
                },
                new()
                {
                    Date = new DateOnly(2025, 10, 11),
                    DocumentType = "availability",
                    Id =  "20251010",
                    Site = "TEST123",
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
                },
                new()
                {
                    Date = new DateOnly(2025, 10, 12),
                    DocumentType = "availability",
                    Id =  "20251010",
                    Site = "TEST123",
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
                }
            });

        var result = await _sut.EditSessionsAsync(site, from, until, sessionMatcher, sessionReplacement);

        result.Success.Should().BeTrue();

        _documentStore.Verify(x => x.PatchDocument(site, It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Exactly(3));
    }
}
