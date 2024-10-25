using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class BookingCosmosDocumentStore(ITypedDocumentCosmosStore<BookingDocument> bookingStore, ITypedDocumentCosmosStore<BookingIndexDocument> indexStore, IMetricsRecorder metricsRecorder) : IBookingsDocumentStore
{
    private const int PointReadLimit = 3;    
           
    public async Task<IEnumerable<Booking>> GetInDateRangeAsync(string site, DateTime from, DateTime to)
    {
        using (metricsRecorder.BeginScope("GetBookingsInDateRange"))
        {
            return await bookingStore.RunQueryAsync<Booking>(b => b.Site == site && b.From >= from && b.From <= to);
        }
    }       
    
    public async Task<Booking> GetByReferenceOrDefaultAsync(string bookingReference)
    {
        try
        {
            var bookingIndexDocument = await indexStore.GetDocument<BookingIndexDocument>(bookingReference);
            var siteId = bookingIndexDocument.Site;
            return await bookingStore.GetDocument<Booking>(bookingReference, siteId);
        }
        catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }
    
    public async Task<IEnumerable<Booking>> GetByNhsNumberAsync(string nhsNumber)
    {
        var bookingIndexDocuments = (await indexStore.RunQueryAsync<BookingIndexDocument>(bi => bi.NhsNumber == nhsNumber)).ToList();
        var results = new List<Booking>();

        var grouped = bookingIndexDocuments.GroupBy(bi => bi.Site);
        foreach (var siteBookings in grouped)
        {
            if (siteBookings.Count() > PointReadLimit)
            {
                var result = await bookingStore.RunQueryAsync<Booking>(b => b.Site == siteBookings.Key && b.AttendeeDetails.NhsNumber == nhsNumber);
                results.AddRange(result);
            }
            else
            {
                foreach (var document in siteBookings)
                {
                    var siteId = document.Site;
                    var bookingReference = document.Reference;
                    var result = await bookingStore.GetDocument<Booking>(bookingReference, siteId); 
                    results.Add(result);
                }
            }
        }
        return results;
    }
    public async Task<bool> UpdateStatus(string bookingReference, string status)
    {
        var bookingIndexDocument = await indexStore.GetDocument<BookingIndexDocument>(bookingReference);
        if(bookingIndexDocument == null)
        {
            return false;
        }
        var updateStatusPatch = PatchOperation.Replace("/outcome", status);
        await bookingStore.PatchDocument(bookingIndexDocument.Site, bookingReference, updateStatusPatch);
        return true;
    }

    public async Task InsertAsync(Booking booking)
    {            
        var bookingDocument = bookingStore.ConvertToDocument(booking);
        await bookingStore.WriteAsync(bookingDocument);

        var bookingIndex = indexStore.ConvertToDocument(booking);
        await indexStore.WriteAsync(bookingIndex);
    }

    public IDocumentUpdate<Booking> BeginUpdate(string site, string reference)
    {
        return new DocumentUpdate<Booking, BookingDocument>(bookingStore, site, reference);
    }
}    