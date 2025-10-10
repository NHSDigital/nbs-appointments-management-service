using System.Linq.Expressions;
using FluentAssertions;
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

    [Fact]
    public async Task ConfirmProvisional_ProvidesCancellationReason_ToPatchOperation()
    {
        var cancellationReason = CancellationReason.CancelledBySite;
        var contactDetails = new List<ContactItem>
        {
            new() { Type = ContactItemType.Email, Value = "some.email@test.com" }
        };
        var bookingIndexDocument = new BookingIndexDocument
        {
            Site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
            DocumentType = "booking_index",
            Id = "01-76-000001",
            NhsNumber = "9999999999",
            Reference = "01-76-000001",
            Status = AppointmentStatus.Provisional
        };
        List<PatchOperation> capturedPatchOperations = null;

        _indexStore.Setup(x => x.GetByIdOrDefaultAsync<BookingIndexDocument>(It.IsAny<string>())).ReturnsAsync(bookingIndexDocument);
        _bookingStore
            .Setup(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()))
            .Callback<string, string, PatchOperation[]>((site, reference, patches) => capturedPatchOperations = [.. patches])
            .ReturnsAsync(() => null);

        var result = await _sut.ConfirmProvisional(bookingIndexDocument.Reference, contactDetails, "another-booking-ref", cancellationReason);

        result.Should().Be(BookingConfirmationResult.Success);
        capturedPatchOperations.Should().NotBeEmpty();
        capturedPatchOperations.Should().Contain(p => p.Path == "/cancellationReason");
    }

    [Fact]
    public async Task CancelAllBookingsInDay_UpdatesBookingStatuses_OfNonCancelledBookings_AndReturnsCorrectCount()
    {
        var bookings = new List<Booking>
        {
            new()
            {
                AttendeeDetails = new()
                {
                    NhsNumber = "9999999999",
                    FirstName = "John",
                    LastName = "Bloggs"
                },
                From = new DateTime(2025, 1, 1, 12, 25, 0),
                Status = AppointmentStatus.Booked,
                Site = "TEST_SITE_123"
            },
            new()
            {
                AttendeeDetails = new()
                {
                    NhsNumber = "9999999999",
                    FirstName = "John",
                    LastName = "Bloggs"
                },
                From = new DateTime(2025, 1, 1, 11, 25, 0),
                Status = AppointmentStatus.Cancelled,
                ContactDetails = [
                    new () {
                        Type = ContactItemType.Phone,
                        Value = "1234567890"
                    }
                ],
                Site = "TEST_SITE_123"
            },
            new()
            {
                AttendeeDetails = new()
                {
                    NhsNumber = "9999999999",
                    FirstName = "John",
                    LastName = "Bloggs"
                },
                From = new DateTime(2025, 1, 1, 10, 25, 0),
                Status = AppointmentStatus.Booked,
                ContactDetails = [
                    new () {
                        Type = ContactItemType.Phone,
                        Value = "1234567890"
                    }
                ],
                Site = "TEST_SITE_123"
            },
            new()
            {
                AttendeeDetails = new()
                {
                    NhsNumber = "9999999999",
                    FirstName = "John",
                    LastName = "Bloggs"
                },
                From = new DateTime(2025, 1, 1, 9, 25, 0),
                Status = AppointmentStatus.Provisional,
                ContactDetails = [
                    new () {
                        Type = ContactItemType.Email,
                        Value = "test.email@domain.com"
                    }
                ],
                Site = "TEST_SITE_123"
            }
        };
        var bookingIndexDocuments = new List<BookingIndexDocument>
        {
            new()
            {
                Site = "TEST_SITE_123",
                DocumentType = "booking_index",
                Id = "01-76-000001",
                NhsNumber = "9999999999",
                Reference = "01-76-000001",
                Status = AppointmentStatus.Booked
            },
            new()
            {
                Site = "TEST_SITE_123",
                DocumentType = "booking_index",
                Id = "02-76-000001",
                NhsNumber = "9999999999",
                Reference = "02-76-000001",
                Status = AppointmentStatus.Cancelled
            },
            new()
            {
                Site = "TEST_SITE_123",
                DocumentType = "booking_index",
                Id = "01-76-000002",
                NhsNumber = "9999999999",
                Reference = "01-76-000002",
                Status = AppointmentStatus.Booked
            },
            new()
            {
                Site = "TEST_SITE_123",
                DocumentType = "booking_index",
                Id = "01-76-000003",
                NhsNumber = "9999999999",
                Reference = "01-76-000003",
                Status = AppointmentStatus.Provisional
            }
        };

        _bookingStore.Setup(x => x.RunQueryAsync<Booking>(It.IsAny<Expression<Func<BookingDocument, bool>>>()))
            .ReturnsAsync(bookings);
        _indexStore.SetupSequence(x => x.GetDocument<BookingIndexDocument>(It.IsAny<string>()))
            .ReturnsAsync(bookingIndexDocuments[0])
            .ReturnsAsync(bookingIndexDocuments[1])
            .ReturnsAsync(bookingIndexDocuments[2])
            .ReturnsAsync(bookingIndexDocuments[3]);

        var (cancelledBookingsCount, bookingsWithoutContactDetailsCount, bookingsWithContactDetails) = await _sut.CancelAllBookingsInDay("TEST_SITE_123", new DateOnly(2025, 1, 1));

        cancelledBookingsCount.Should().Be(2);
        bookingsWithoutContactDetailsCount.Should().Be(1);
        bookingsWithContactDetails.Count.Should().Be(1);

        _bookingStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CancelAllBookingsInDay_DoesNotUpdateBookingStatuses_WithNoBookings()
    {
        _bookingStore.Setup(x => x.RunQueryAsync<Booking>(It.IsAny<Expression<Func<BookingDocument, bool>>>()))
            .ReturnsAsync(new List<Booking>());

        var (cancelledBookingsCount, bookingsWithoutContactDetailsCount, bookingsWithContactDetails) = await _sut.CancelAllBookingsInDay("TEST_SITE_123", new DateOnly(2025, 1, 1));

        cancelledBookingsCount.Should().Be(0);
        bookingsWithoutContactDetailsCount.Should().Be(0);
        bookingsWithContactDetails.Should().BeEmpty();

        _bookingStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Never);
    }

    [Fact]
    public async Task GetRecentlyUpdatedBookingsCrossSiteAsync_ThrowsException_WhenNoStatusesPassedIn()
    {
        var act = async () => await _sut.GetRecentlyUpdatedBookingsCrossSiteAsync(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("You must specify one or more statuses.");
    }

    [Fact]
    public async Task GetRecentlyUpdatedBookingsCrossSiteAsync_ReturnsBookings()
    {
        var statusUpdated = DateTime.Now.AddDays(-1);
        var bookingIndexDocuments = new List<BookingIndexDocument>
        {
            new()
            {
                Site = "TEST_SITE_123",
                DocumentType = "booking_index",
                Id = "01-76-000001",
                NhsNumber = "9999999999",
                Reference = "01-76-000001",
                Status = AppointmentStatus.Cancelled,
                StatusUpdated = statusUpdated
            },
            new()
            {
                Site = "TEST_SITE_123",
                DocumentType = "booking_index",
                Id = "02-76-000001",
                NhsNumber = "9999999999",
                Reference = "02-76-000001",
                Status = AppointmentStatus.Cancelled,
                StatusUpdated = statusUpdated
            },
            new()
            {
                Site = "ANOTHER_TEST_SITE",
                DocumentType = "booking_index",
                Id = "01-76-000002",
                NhsNumber = "9999999999",
                Reference = "01-76-000002",
                Status = AppointmentStatus.Cancelled,
                StatusUpdated = statusUpdated
            },
            new()
            {
                Site = "ANOTHER_TEST_SITE",
                DocumentType = "booking_index",
                Id = "01-76-000003",
                NhsNumber = "9999999999",
                Reference = "01-76-000003",
                Status = AppointmentStatus.Cancelled,
                StatusUpdated = statusUpdated
            }
        };
        var bookings = new List<Booking>
        {
            new()
            {
                AttendeeDetails = new()
                {
                    NhsNumber = "9999999999",
                    FirstName = "John",
                    LastName = "Bloggs"
                },
                From = new DateTime(2025, 1, 1, 12, 25, 0),
                Status = AppointmentStatus.Booked,
                Site = "TEST_SITE_123"
            },
            new()
            {
                AttendeeDetails = new()
                {
                    NhsNumber = "9999999999",
                    FirstName = "John",
                    LastName = "Bloggs"
                },
                From = new DateTime(2025, 1, 1, 11, 25, 0),
                Status = AppointmentStatus.Cancelled,
                ContactDetails = [
                    new () {
                        Type = ContactItemType.Phone,
                        Value = "1234567890"
                    }
                ],
                Site = "TEST_SITE_123"
            },
            new()
            {
                AttendeeDetails = new()
                {
                    NhsNumber = "9999999999",
                    FirstName = "John",
                    LastName = "Bloggs"
                },
                From = new DateTime(2025, 1, 1, 10, 25, 0),
                Status = AppointmentStatus.Booked,
                ContactDetails = [
                    new () {
                        Type = ContactItemType.Phone,
                        Value = "1234567890"
                    }
                ],
                Site = "ANOTHER_TEST_SITE"
            },
            new()
            {
                AttendeeDetails = new()
                {
                    NhsNumber = "9999999999",
                    FirstName = "John",
                    LastName = "Bloggs"
                },
                From = new DateTime(2025, 1, 1, 9, 25, 0),
                Status = AppointmentStatus.Provisional,
                ContactDetails = [
                    new () {
                        Type = ContactItemType.Email,
                        Value = "test.email@domain.com"
                    }
                ],
                Site = "ANOTHER_TEST_SITE"
            }
        };

        _indexStore.Setup(x => x.RunQueryAsync<BookingIndexDocument>(It.IsAny<Expression<Func<BookingIndexDocument, bool>>>()))
            .ReturnsAsync(bookingIndexDocuments);
        _bookingStore.SetupSequence(x => x.RunQueryAsync<Booking>(It.IsAny<Expression<Func<BookingDocument, bool>>>()))
            .ReturnsAsync(bookings.Take(2))
            .ReturnsAsync(bookings.Skip(2).Take(2));


        var result = await _sut.GetRecentlyUpdatedBookingsCrossSiteAsync(statusUpdated, statusUpdated.AddDays(1), AppointmentStatus.Cancelled);

        result.Count().Should().Be(4);
    }
}
