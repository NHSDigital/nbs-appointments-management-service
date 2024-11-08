using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using System.Collections.Concurrent;

namespace Nhs.Appointments.Persistance;

public class BookingCosmosDocumentStore(ITypedDocumentCosmosStore<BookingDocument> bookingStore, ITypedDocumentCosmosStore<BookingIndexDocument> indexStore, IMetricsRecorder metricsRecorder, TimeProvider time) : IBookingsDocumentStore
{
    private const int PointReadLimit = 3;    
           
    public async Task<IEnumerable<Booking>> GetInDateRangeAsync(DateTime from, DateTime to, string site)
    {
        using (metricsRecorder.BeginScope("GetBookingsInDateRange"))
        {
            return await bookingStore.RunQueryAsync<Booking>(b => b.Site == site && b.From >= from && b.From <= to);
        }
    }

    public async Task<IEnumerable<Booking>> GetCrossSiteAsync(DateTime from, DateTime to, bool provisional = false)
    {
        var bookingIndexDocuments = await indexStore.RunQueryAsync<BookingIndexDocument>(i => i.From >= from && i.From <= to && i.Provisional == provisional);
        var grouped = bookingIndexDocuments.GroupBy(i => i.Site);

        var concurrentResults = new ConcurrentBag<IEnumerable<Booking>>();

        await Parallel.ForEachAsync(grouped, async (group, _) =>
        {
            var bookings = await GetInDateRangeAsync(group.Min(g => g.From), group.Max(g => g.From), group.Key);
            concurrentResults.Add(bookings);
        });

        return concurrentResults.SelectMany(x => x);
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

        var grouped = bookingIndexDocuments.Where(bi => bi.Provisional == false).GroupBy(bi => bi.Site);
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

    public async Task<BookingConfirmationResult> ConfirmProvisional(string bookingReference, IEnumerable<ContactItem> contactDetails)
    {
        var bookingIndexDocument = await indexStore.GetByIdOrDefaultAsync<BookingIndexDocument>(bookingReference);
        if (bookingIndexDocument != null)
        {
            if (bookingIndexDocument.Created.AddMinutes(5) < time.GetUtcNow())
                return BookingConfirmationResult.Expired;

            var updateStatusPatch = PatchOperation.Replace("/provisional", false);
            var addContactDetailsPath = PatchOperation.Add("/contactDetails", contactDetails);
            await indexStore.PatchDocument("booking_index", bookingReference, updateStatusPatch);
            await bookingStore.PatchDocument(bookingIndexDocument.Site, bookingReference, updateStatusPatch, addContactDetailsPath);
            return BookingConfirmationResult.Success;
        }

        return BookingConfirmationResult.NotFound;
    }


    public async Task SetReminderSent(string bookingReference, string site)
    {
        var patch = PatchOperation.Set("/reminderSent", true);
        await bookingStore.PatchDocument(site, bookingReference, patch);
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

    public async Task RemoveUnconfirmedProvisionalBookings()
    {
        var indexDocuments = await indexStore.RunQueryAsync<BookingIndexDocument>(i => i.Provisional && i.Created <= time.GetUtcNow().AddMinutes(-5));
        foreach (var indexDocument in indexDocuments)
        {
            await indexStore.DeleteDocument(indexDocument.Reference, "booking_index");
            await bookingStore.DeleteDocument(indexDocument.Reference, indexDocument.Site);
        }
    }
}    