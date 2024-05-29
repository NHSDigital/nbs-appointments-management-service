using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class BookingCosmosDocumentStore : IBookingsDocumentStore
{
    private const int PointReadLimit = 3;
    private readonly ITypedDocumentCosmosStore<BookingDocument> _bookingStore;
    private readonly ITypedDocumentCosmosStore<BookingIndexDocument> _indexStore;

    public BookingCosmosDocumentStore(ITypedDocumentCosmosStore<BookingDocument> bookingStore, ITypedDocumentCosmosStore<BookingIndexDocument> indexStore) 
    { 
        _bookingStore = bookingStore;
        _indexStore = indexStore;
    }
           
    public Task<IEnumerable<Booking>> GetInDateRangeAsync(string site, DateTime from, DateTime to)
    {
        return _bookingStore.RunQueryAsync<Booking>(b => b.Site == site && b.From >= from && b.From <= to);
    }       
    
    public async Task<Booking> GetByReferenceOrDefaultAsync(string bookingReference)
    {
        try
        {
            var bookingIndexDocument = await _indexStore.GetDocument<BookingIndexDocument>(bookingReference);
            var siteId = bookingIndexDocument.Site;
            return await _bookingStore.GetDocument<Booking>(bookingReference, siteId);
        }
        catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }
    
    public async Task<IEnumerable<Booking>> GetByNhsNumberAsync(string nhsNumber)
    {
        var bookingIndexDocuments = (await _indexStore.RunQueryAsync<BookingIndexDocument>(bi => bi.NhsNumber == nhsNumber)).ToList();
        var results = new List<Booking>();

        var grouped = bookingIndexDocuments.GroupBy(bi => bi.Site);
        foreach (var siteBookings in grouped)
        {
            if (siteBookings.Count() > PointReadLimit)
            {
                var result = await _bookingStore.RunQueryAsync<Booking>(b => b.Site == siteBookings.Key && b.AttendeeDetails.NhsNumber == nhsNumber);
                results.AddRange(result);
            }
            else
            {
                foreach (var document in siteBookings)
                {
                    var siteId = document.Site;
                    var bookingReference = document.Reference;
                    var result = await _bookingStore.GetDocument<Booking>(bookingReference, siteId); 
                    results.Add(result);
                }
            }
        }
        return results;
    }
    public async Task<bool> UpdateStatus(string bookingReference, string status)
    {
        var bookingIndexDocument = await _indexStore.GetDocument<BookingIndexDocument>(bookingReference);
        if(bookingIndexDocument == null)
        {
            return false;
        }
        var updateStatusPatch = PatchOperation.Replace("/outcome", status);
        await _bookingStore.PatchDocument(bookingIndexDocument.Site, bookingReference, updateStatusPatch);
        return true;
    }

    public async Task InsertAsync(Booking booking)
    {            
        var bookingDocument = _bookingStore.ConvertToDocument(booking);
        await _bookingStore.WriteAsync(bookingDocument);

        var bookingIndex = _indexStore.ConvertToDocument(booking);
        await _indexStore.WriteAsync(bookingIndex);
    }

    public IDocumentUpdate<Booking> BeginUpdate(string site, string reference)
    {
        return new DocumentUpdate<Booking, BookingDocument>(_bookingStore, site, reference);
    }
}    