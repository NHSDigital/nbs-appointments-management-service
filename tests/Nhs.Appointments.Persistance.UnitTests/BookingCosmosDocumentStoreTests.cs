using System.Linq.Expressions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests;

public class BookingCosmosDocumentStoreTests
{
    private readonly BookingCosmosDocumentStore _sut;
    private readonly Mock<ITypedDocumentCosmosStore<BookingDocument>> _bookingStore = new();
    private readonly Mock<ITypedDocumentCosmosStore<BookingIndexDocument>> _indexStore = new();

    public BookingCosmosDocumentStoreTests()
    {
        _sut = new BookingCosmosDocumentStore(_bookingStore.Object, _indexStore.Object, TimeProvider.System);
    }
    
    [Fact]
    public async Task GetByNhsNumberAsync_CallsRunQueryAsync_WhenBookingCountIsGreaterThanPointReadLimit()
    {
        var bookingIndexDocuments = new List<BookingIndexDocument>
        {
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "01-76-000001", NhsNumber = "9999999999", Reference = "01-76-000001" },
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "01-76-000002", NhsNumber = "9999999999", Reference = "01-76-000002" },
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "01-76-000003", NhsNumber = "9999999999", Reference = "01-76-000003" },
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "02-76-000004", NhsNumber = "9999999999", Reference = "02-76-000004" },
        };

        _indexStore.Setup(x => 
                x.RunQueryAsync<BookingIndexDocument>(It.IsAny<Expression<Func<BookingIndexDocument, bool>>>()))
            .ReturnsAsync(bookingIndexDocuments);
        
        await _sut.GetByNhsNumberAsync("9999999999");
        _bookingStore.Verify(x => x.RunQueryAsync<Booking>(It.IsAny<Expression<Func<BookingDocument, bool>>>()), Times.Once);
    }
    
    [Fact]
    public async Task GetByNhsNumberAsync_CallsGetDocument_WhenBookingCountIsLessThanPointReadLimit()
    {
        var bookingIndexDocuments = new List<BookingIndexDocument>
        {
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "01-76-000001", NhsNumber = "9999999999", Reference = "01-76-000001" },
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "01-76-000002", NhsNumber = "9999999999", Reference = "01-76-000002" },
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
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "01-76-000001", NhsNumber = "9999999999", Reference = "01-76-000001" },
            new BookingIndexDocument {Site = "1001", DocumentType = "booking_index", Id = "02-76-000001", NhsNumber = "9999999999", Reference = "02-76-000001" },
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "01-76-000002", NhsNumber = "9999999999", Reference = "01-76-000002" },
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "01-76-000003", NhsNumber = "9999999999", Reference = "01-76-000003" },
            new BookingIndexDocument {Site = "1000", DocumentType = "booking_index", Id = "02-76-000004", NhsNumber = "9999999999", Reference = "02-76-000004" },
            new BookingIndexDocument {Site = "1001", DocumentType = "booking_index", Id = "02-76-000002", NhsNumber = "9999999999", Reference = "02-76-000002" },
        };

        _indexStore.Setup(x => 
                x.RunQueryAsync<BookingIndexDocument>(It.IsAny<Expression<Func<BookingIndexDocument, bool>>>()))
            .ReturnsAsync(bookingIndexDocuments);
        
        await _sut.GetByNhsNumberAsync("9999999999");
        _bookingStore.Verify(x => x.RunQueryAsync<Booking>(It.IsAny<Expression<Func<BookingDocument, bool>>>()), Times.Once);
        _bookingStore.Verify(x => x.GetDocument<Booking>(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }
}