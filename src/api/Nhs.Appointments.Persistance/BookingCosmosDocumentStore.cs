using System.Collections.Concurrent;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class BookingCosmosDocumentStore(
    ITypedDocumentCosmosStore<BookingDocument> bookingStore, 
    ITypedDocumentCosmosStore<BookingIndexDocument> indexStore, 
    IMetricsRecorder metricsRecorder, 
    TimeProvider time,
    ILogger<BookingCosmosDocumentStore> logger
) : IBookingsDocumentStore
{
    private const int PointReadLimit = 3;    
           
    public async Task<IEnumerable<Booking>> GetInDateRangeAsync(DateTime from, DateTime to, string site)
    {
        using (metricsRecorder.BeginScope("GetBookingsInDateRange"))
        {
            return await bookingStore.RunQueryAsync<Booking>(b => b.DocumentType == "booking" && b.Site == site && b.From >= from && b.From <= to);
        }
    }

    public async Task<IEnumerable<Booking>> QueryByFilterAsync(BookingQueryFilter queryFilter)
    {
        // TODO: Rather than returning more results than we need then querying in memory, I wanted the filter
        // to be passed in as the predicate like so:
        // return await bookingStore.RunQueryAsync<Booking>(queryFilter.Matches);
        // Unfortunately, CosmosDB throws a Method Unsupported error the second you throw anything precompiled at it.
        // This will ONLY work if you write the predicate inline, at which point you lose testability and abstraction.

        using (metricsRecorder.BeginScope("QueryByFilter"))
        {
            var rawResults = await bookingStore.RunQueryAsync<Booking>(b =>
                b.DocumentType == "booking" && b.Site == queryFilter.Site && b.From >= queryFilter.StartsAtOrAfter &&
                b.From <= queryFilter.StartsAtOrBefore);

            return rawResults.Where(queryFilter.Matches);
        }
    }

    public async Task<IEnumerable<Booking>> GetCrossSiteAsync(DateTime from, DateTime to, params AppointmentStatus[] statuses)
    {
        if (statuses.Length == 0)
            throw new ArgumentException("You must specify one or more statuses");

        var bookingIndexDocuments = await indexStore.RunQueryAsync<BookingIndexDocument>(i => i.DocumentType == "booking_index" && i.From >= from && i.From <= to);
        var siteGroupedBookings = bookingIndexDocuments.Where(b => statuses.Contains(b.Status)).GroupBy(i => i.Site);

        var concurrentResults = new ConcurrentBag<IEnumerable<Booking>>();

        await Parallel.ForEachAsync(siteGroupedBookings, async (group, _) =>
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
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }
    
    public async Task<IEnumerable<Booking>> GetByNhsNumberAsync(string nhsNumber)
    {
        var bookingIndexDocuments = (await indexStore.RunQueryAsync<BookingIndexDocument>(bi => bi.DocumentType == "booking_index" && bi.NhsNumber == nhsNumber)).ToList();
        var results = new List<Booking>();

        var grouped = bookingIndexDocuments.Where(bi => bi.Status == AppointmentStatus.Booked).GroupBy(bi => bi.Site);
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

                    try
                    {
                        var result = await bookingStore.GetDocument<Booking>(bookingReference, siteId);
                        if (result != null)
                        {
                            results.Add(result);
                        }
                    }
                    catch (Exception ex) {
                        logger.LogError(ex, "Did not find booking: {BookingReference} in booking container", bookingReference);
                    }
                }
            }
        }
        return results;
    }

    public async Task<bool> UpdateStatus(string bookingReference, AppointmentStatus status,
        AvailabilityStatus availabilityStatus, CancellationReason? cancellationReason = null)
    {
        var bookingIndexDocument = await indexStore.GetDocument<BookingIndexDocument>(bookingReference);
        if(bookingIndexDocument == null)
        {
            return false;
        }

        await UpdateStatus(bookingIndexDocument, status, availabilityStatus, cancellationReason);
        return true;
    }

    public async Task<bool> UpdateAvailabilityStatus(string bookingReference, AvailabilityStatus status)
    {
        var bookingIndexDocument = await indexStore.GetDocument<BookingIndexDocument>(bookingReference);
        if (bookingIndexDocument == null)
        {
            return false;
        }

        await UpdateAvailabilityStatus(bookingIndexDocument, status);
        return true;
    }

    private async Task UpdateStatus(BookingIndexDocument booking, AppointmentStatus status,
        AvailabilityStatus availabilityStatus, CancellationReason? cancellationReason = null)
    {
        var updateStatusPatch = PatchOperation.Replace("/status", status);
        var statusUpdatedPatch = PatchOperation.Replace("/statusUpdated", time.GetUtcNow());
        var updateAvailabilityStatusPatch =
            PatchOperation.Replace("/availabilityStatus", availabilityStatus);

        await indexStore.PatchDocument("booking_index", booking.Reference, updateStatusPatch);

        var patchOperations = new List<PatchOperation>
        {
            updateStatusPatch,
            statusUpdatedPatch,
            updateAvailabilityStatusPatch
        };

        if (cancellationReason != null)
        {
            patchOperations.Add(PatchOperation.Add("/cancellationReason", cancellationReason));
        }

        await bookingStore.PatchDocument(booking.Site, booking.Reference, patchOperations.ToArray());
    }

    private async Task UpdateAvailabilityStatus(BookingIndexDocument booking, AvailabilityStatus status)
    {
        var updateStatusPatch = PatchOperation.Replace("/availabilityStatus", status);
        await bookingStore.PatchDocument(booking.Site, booking.Reference, updateStatusPatch);
    }

    private async Task<(BookingConfirmationResult, BookingIndexDocument)> GetBookingForReschedule(string bookingReference, string nhsNumber)
    {
        if (string.IsNullOrEmpty(bookingReference) == false)
        {
            var rescheduleDocument = await indexStore.GetByIdOrDefaultAsync<BookingIndexDocument>(bookingReference);
            if (rescheduleDocument == null)
            {
                return (BookingConfirmationResult.RescheduleNotFound, null);
            }

            if (rescheduleDocument.NhsNumber != nhsNumber)
            {
                return (BookingConfirmationResult.RescheduleMismatch, null);
            }
            
            return (BookingConfirmationResult.Unknown, rescheduleDocument);
        }
        
        return (BookingConfirmationResult.Unknown, null);
    }

    public async Task<BookingConfirmationResult> ConfirmProvisionals(string[] bookingReferences, IEnumerable<ContactItem> contactDetails)
    {
        var bookingBatchSize = bookingReferences.Length;
        var bookingDocuments = new List<BookingIndexDocument>();

        foreach (var reference in bookingReferences) 
        {
            var childIndexDocument = await indexStore.GetByIdOrDefaultAsync<BookingIndexDocument>(reference);
            var childProvisionalValidationResult = ValidateBookingDocumentProvisionalState(childIndexDocument);

            if (childProvisionalValidationResult != BookingConfirmationResult.Success)
            {
                return BookingConfirmationResult.GroupBookingInvalid;
            }

            bookingDocuments.Add(childIndexDocument);
        }

        foreach (var document in bookingDocuments) 
        {
            await PatchProvisionalToConfirmed(document, contactDetails, bookingBatchSize);
        }

        return BookingConfirmationResult.Success;
    }

    public async Task<BookingConfirmationResult> ConfirmProvisional(
        string bookingReference,
        IEnumerable<ContactItem> contactDetails,
        string? bookingToReschedule,
        CancellationReason? cancellationReason = null)
    {
        var bookingIndexDocument = await indexStore.GetByIdOrDefaultAsync<BookingIndexDocument>(bookingReference);
        var provisionalValidationResult = ValidateBookingDocumentProvisionalState(bookingIndexDocument);

        if (provisionalValidationResult != BookingConfirmationResult.Success)
        {
            return provisionalValidationResult;
        }

        var (getRescheduleResult, rescheduleDocument) = await GetBookingForReschedule(bookingToReschedule, bookingIndexDocument.NhsNumber);

        if (getRescheduleResult != BookingConfirmationResult.Unknown)
            return getRescheduleResult;

        await PatchProvisionalToConfirmed(bookingIndexDocument, contactDetails);

        if (rescheduleDocument != null)
        {
            await UpdateStatus(rescheduleDocument, AppointmentStatus.Cancelled, AvailabilityStatus.Unknown, cancellationReason);
        }
        
        return BookingConfirmationResult.Success;
    }

    private async Task PatchProvisionalToConfirmed(BookingIndexDocument bookingIndexDocument,
        IEnumerable<ContactItem> contactDetails, int? bookingBatchSize = null)
    {
        var updateStatusPatch = PatchOperation.Replace("/status", AppointmentStatus.Booked);
        var statusUpdatedPatch = PatchOperation.Replace("/statusUpdated", time.GetUtcNow());
        var addContactDetailsPath = PatchOperation.Add("/contactDetails", contactDetails);

        await indexStore.PatchDocument("booking_index", bookingIndexDocument.Reference, updateStatusPatch);

        if (bookingBatchSize.HasValue)
        {
            var bookingBatchSizePatch = PatchOperation.Add("/bookingBatchSize", bookingBatchSize);
            await bookingStore.PatchDocument(bookingIndexDocument.Site, bookingIndexDocument.Reference,
                updateStatusPatch, statusUpdatedPatch, addContactDetailsPath, bookingBatchSizePatch);
        }
        else
        {
            await bookingStore.PatchDocument(bookingIndexDocument.Site, bookingIndexDocument.Reference,
                updateStatusPatch, statusUpdatedPatch, addContactDetailsPath);
        }

        // TODO: Once integration tests are in place, try this instead of the if statement:
        // await bookingStore.PatchDocument(bookingIndexDocument.Site, bookingIndexDocument.Reference,
        //     updateStatusPatch, statusUpdatedPatch, addContactDetailsPath,
        //     bookingBatchSize.HasValue ? PatchOperation.Add("/bookingBatchSize", bookingBatchSize) : null);
    }

    private BookingConfirmationResult ValidateBookingDocumentProvisionalState(BookingIndexDocument document) 
    {
        if (document == null)
        {
            return BookingConfirmationResult.NotFound;
        }
        if (document.Status is not AppointmentStatus.Provisional)
        {
            return BookingConfirmationResult.StatusMismatch;
        }

        if (document.Created.AddMinutes(5) < time.GetUtcNow())
        {
            return BookingConfirmationResult.Expired;
        }

        return BookingConfirmationResult.Success;
    }


    public async Task SetReminderSent(string bookingReference, string site)
    {
        var patch = PatchOperation.Set("/reminderSent", true);
        await bookingStore.PatchDocument(site, bookingReference, patch);
    }

    public async Task InsertAsync(Booking booking)
    {
        var bookingDocument = bookingStore.ConvertToDocument(booking);
        bookingDocument.StatusUpdated = time.GetUtcNow();
        await bookingStore.WriteAsync(bookingDocument);

        var bookingIndex = indexStore.ConvertToDocument(booking);
        await indexStore.WriteAsync(bookingIndex);
    }

    public IDocumentUpdate<Booking> BeginUpdate(string site, string reference)
    {
        return new DocumentUpdate<Booking, BookingDocument>(bookingStore, site, reference);
    }

    public async Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings()
    {
        var expiryDateTime = time.GetUtcNow().AddDays(-1);        

        var query = new QueryDefinition(
                query: "SELECT * " +
                       "FROM index_data d " +
                       "WHERE d.docType = @docType AND d.status = @status AND d.created < @expiry")
            .WithParameter("@docType", "booking_index")
            .WithParameter("@status", AppointmentStatus.Provisional.ToString())
            .WithParameter("@expiry", expiryDateTime);
        var indexDocuments = await indexStore.RunSqlQueryAsync<BookingIndexDocument>(query);

        foreach (var indexDocument in indexDocuments)
        {
            await DeleteBooking(indexDocument.Reference, indexDocument.Site);
        }

        return indexDocuments.Select(i => i.Reference).ToList();
    }

    public async Task DeleteBooking(string reference, string site) => await Task.WhenAll(
        indexStore.DeleteDocument(reference, "booking_index"),
        bookingStore.DeleteDocument(reference, site)
    );

    public async Task<(int cancelledBookingsCount, int bookingsWithoutContactDetailsCount, List<Booking> bookingsWithContactDetails)> CancelAllBookingsInDay(string site, DateOnly date)
    {
        using (metricsRecorder.BeginScope("CancelAllBookingsInDay"))
        {
            var startOfDay = date.ToDateTime(TimeOnly.MinValue);
            var endOfDay = date.AddDays(1).ToDateTime(TimeOnly.MinValue).AddTicks(-1);

            var bookings = await GetInDateRangeAsync(startOfDay, endOfDay, site);

            var successfulCancellations = 0;
            var bookingsWithoutContactDetailsCount = 0;

            foreach (var booking in bookings)
            {
                if (await UpdateStatus(booking.Reference, AppointmentStatus.Cancelled, AvailabilityStatus.Unknown, CancellationReason.CancelledBySite))
                {
                    successfulCancellations++;
                }

                if (booking.ContactDetails is null || booking.ContactDetails.Length == 0)
                {
                    bookingsWithoutContactDetailsCount++;
                }
            }

            return (successfulCancellations, bookingsWithoutContactDetailsCount, bookings.Where(b => b.ContactDetails is not null).ToList());
        }
    }
}    
