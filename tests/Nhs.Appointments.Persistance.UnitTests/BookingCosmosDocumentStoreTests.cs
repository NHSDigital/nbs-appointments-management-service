using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests;

public class BookingCosmosDocumentStoreTests
{
    private readonly Mock<ITypedDocumentCosmosStore<BookingDocument>> _bookingStore = new();
    private readonly Mock<ITypedDocumentCosmosStore<BookingIndexDocument>> _indexStore = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ILogger<BookingCosmosDocumentStore>> _logger = new();
    private readonly BookingCosmosDocumentStore _sut;
    private readonly Mock<TimeProvider> _timeProvider = new();

    public BookingCosmosDocumentStoreTests()
    {
        _sut = new BookingCosmosDocumentStore(
            _bookingStore.Object, 
            _indexStore.Object, 
            _metricsRecorder.Object,
            _timeProvider.Object, 
            _logger.Object);
    }

    [Fact]
    public async Task GetByNhsNumberAsync_CallsRunQueryAsync_WhenBookingCountIsGreaterThanPointReadLimit()
    {
        var bookingIndexDocuments = new List<BookingIndexDocument>
        {
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000001",
                NhsNumber = "9999999999",
                Reference = "01-76-000001",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000002",
                NhsNumber = "9999999999",
                Reference = "01-76-000002",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000003",
                NhsNumber = "9999999999",
                Reference = "01-76-000003",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "02-76-000004",
                NhsNumber = "9999999999",
                Reference = "02-76-000004",
                Status = AppointmentStatus.Booked
            },
        };

        _indexStore.Setup(x =>
                x.RunQueryAsync<BookingIndexDocument>(It.IsAny<Expression<Func<BookingIndexDocument, bool>>>()))
            .ReturnsAsync(bookingIndexDocuments);

        await _sut.GetByNhsNumberAsync("9999999999");
        _bookingStore.Verify(x => x.RunQueryAsync<Booking>(It.IsAny<Expression<Func<BookingDocument, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByNhsNumberAsync_CallsGetDocument_WhenBookingCountIsLessThanPointReadLimit()
    {
        var bookingIndexDocuments = new List<BookingIndexDocument>
        {
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000001",
                NhsNumber = "9999999999",
                Reference = "01-76-000001",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000002",
                NhsNumber = "9999999999",
                Reference = "01-76-000002",
                Status = AppointmentStatus.Booked
            },
        };

        _indexStore.Setup(x =>
                x.RunQueryAsync<BookingIndexDocument>(It.IsAny<Expression<Func<BookingIndexDocument, bool>>>()))
            .ReturnsAsync(bookingIndexDocuments);

        await _sut.GetByNhsNumberAsync("9999999999");
        _bookingStore.Verify(x => x.GetDocument<Booking>(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetByNhsNumberAsync_GroupsSitesAndCallsCorrectMethod_WhenThereAreBookingForMultipleSites()
    {
        var bookingIndexDocuments = new List<BookingIndexDocument>
        {
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000001",
                NhsNumber = "9999999999",
                Reference = "01-76-000001",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "34e990af-5dc9-43a6-8895-b9123216d699",
                DocumentType = "booking_index",
                Id = "02-76-000001",
                NhsNumber = "9999999999",
                Reference = "02-76-000001",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000002",
                NhsNumber = "9999999999",
                Reference = "01-76-000002",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000003",
                NhsNumber = "9999999999",
                Reference = "01-76-000003",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "02-76-000004",
                NhsNumber = "9999999999",
                Reference = "02-76-000004",
                Status = AppointmentStatus.Booked
            },
            new BookingIndexDocument
            {
                Site = "34e990af-5dc9-43a6-8895-b9123216d699",
                DocumentType = "booking_index",
                Id = "02-76-000002",
                NhsNumber = "9999999999",
                Reference = "02-76-000002",
                Status = AppointmentStatus.Booked
            },
        };

        _indexStore.Setup(x =>
                x.RunQueryAsync<BookingIndexDocument>(It.IsAny<Expression<Func<BookingIndexDocument, bool>>>()))
            .ReturnsAsync(bookingIndexDocuments);

        await _sut.GetByNhsNumberAsync("9999999999");
        _bookingStore.Verify(x => x.RunQueryAsync<Booking>(It.IsAny<Expression<Func<BookingDocument, bool>>>()),
            Times.Once);
        _bookingStore.Verify(x => x.GetDocument<Booking>(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }


    [Fact]
    public async Task GetByNhsNumberAsync_LogsError_WhenBookingNotFoundInBookingContainer()
    {
        var bookingIndexDocuments = new List<BookingIndexDocument>
        {
            new BookingIndexDocument
            {
                Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                DocumentType = "booking_index",
                Id = "01-76-000001",
                NhsNumber = "9999999999",
                Reference = "01-76-000001",
                Status = AppointmentStatus.Booked
            }
        };

        _indexStore.Setup(x =>
                x.RunQueryAsync<BookingIndexDocument>(It.IsAny<Expression<Func<BookingIndexDocument, bool>>>()))
            .ReturnsAsync(bookingIndexDocuments);
        _bookingStore.Setup(x => x.GetDocument<Booking>(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());
        var bookingReference = bookingIndexDocuments[0].Reference;

        await _sut.GetByNhsNumberAsync("9999999999");

        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains($"Did not find booking: {bookingReference} in booking container")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );
    }

    [Fact]
    public async Task UpdateStatus_CancellationReasonIsNull_CancellationReasonPatchNotIncluded()
    {
        var bookingReference = "booking-ref";
        var appointmentStatus = AppointmentStatus.Unknown;
        var availabilityStatus = AvailabilityStatus.Unknown;
        var bookingIndexDocument = new BookingIndexDocument();
        List<PatchOperation> capturedPatchOperations = null;
        
        _indexStore.Setup(x => x.GetDocument<BookingIndexDocument>(It.IsAny<string>())).ReturnsAsync(bookingIndexDocument);
        _bookingStore
            .Setup(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()))
            .Callback<string, string, PatchOperation[]>((site, reference, patches) =>
            {
                capturedPatchOperations = patches.ToList();
            })
            .ReturnsAsync(() => null);

        // Act
        await _sut.UpdateStatus(bookingReference, appointmentStatus, availabilityStatus, cancellationReason: null);

        // Assert
        Assert.NotNull(capturedPatchOperations);
        Assert.DoesNotContain(capturedPatchOperations, p => p.Path == "/cancellationReason");
    }

    [Fact]
    public async Task UpdateStatus_CancellationReasonProvided_CancellationReasonPatchIncluded()
    {
        var bookingReference = "booking-ref";
        var appointmentStatus = AppointmentStatus.Unknown;
        var availabilityStatus = AvailabilityStatus.Unknown;
        var cancellationReason = CancellationReason.CancelledBySite;
        var bookingIndexDocument = new BookingIndexDocument();
        List<PatchOperation> capturedPatchOperations = null;

        _indexStore.Setup(x => x.GetDocument<BookingIndexDocument>(It.IsAny<string>())).ReturnsAsync(bookingIndexDocument);
        _bookingStore
            .Setup(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()))
            .Callback<string, string, PatchOperation[]>((site, reference, patches) =>
            {
                capturedPatchOperations = patches.ToList();
            })
            .ReturnsAsync(() => null);

        // Act
        await _sut.UpdateStatus(bookingReference, appointmentStatus, availabilityStatus, cancellationReason);

        // Assert
        Assert.NotNull(capturedPatchOperations);
        Assert.Contains(capturedPatchOperations, p => p.Path == "/cancellationReason");
    }
}
