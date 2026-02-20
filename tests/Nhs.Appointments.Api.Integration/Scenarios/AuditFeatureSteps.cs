using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using Location = Gherkin.Ast.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract class AuditFeatureSteps : AggregateFeatureSteps
{
    private const string AuditContainer = "audit_data";
    private const string CoreContainer = "core_data";
    private const string BookingContainer = "booking_data";
    
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);
    private readonly TimeSpan _pollingTimeout = TimeSpan.FromSeconds(20);

    [Then(@"an audit function document for the default site was created for user '(.+)' and function '(.+)'")]
    [And(@"an audit function document for the default site was created for user '(.+)' and function '(.+)'")]
    public async Task AssertAuditFunctionDocumentCreated(string user, string function) =>
        await AssertAuditFunctionDocumentExists(user, function, GetSiteId());

    [Then(@"an audit function document was created for user '(.+)' and function '(.+)' and no site")]
    [And(@"an audit function document was created for user '(.+)' and function '(.+)' and no site")]
    public async Task AssertAuditFunctionDocumentCreatedWithNoSite(string user, string function) =>
        await AssertAuditFunctionDocumentExists(user, function, null);

    private async Task AssertAuditFunctionDocumentExists(string user, string functionName, string siteId)
    {
        var timestamp = DateTime.UtcNow;

        var auditFunctionDocument = await FindAuditFunctionTimestampedWithRetryAsync(user, functionName, siteId);

        //confirm that the document was created recently, as cannot easily verify via id query
        auditFunctionDocument.Timestamp.Should().BeBefore(timestamp);
        auditFunctionDocument.Timestamp.Should().BeOnOrAfter(timestamp.Add(-1 * _pollingTimeout));
    }
    
    [And("the original booking with details has a sanitized audit and index in blob storage at the default site")]
    public async Task AssertOriginalAuditBookingAndIndex(DataTable dataTable)
    {
        //TODO this reference manager needs fixing across the board!!
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertAuditBookingAndIndex(bookingReference, dataTable);
    }
    
    [And("the rescheduled booking with details has a sanitized audit and index in blob storage at the default site")]
    public async Task AssertRescheduleAuditBookingAndIndex(DataTable dataTable)
    {
        await AssertAuditBookingAndIndex(_reschduledBookingReference, dataTable);
    }
    
    private async Task AssertAuditBookingAndIndex(string reference, DataTable dataTable)
    {
        var bookingDetails = dataTable.Rows.Skip(1).Take(1).Select((row, _) =>
        {
            var status = dataTable.GetEnumRowValue(row, "Appointment Status", AppointmentStatus.Unknown);
            var availabilityStatus = dataTable.GetEnumRowValue(row, "Availability Status", AvailabilityStatus.Unknown);
            var lastUpdatedBy = dataTable.GetRowValueOrDefault(row, "Last Updated By");

            return new BookingDocument
            {
                Reference = reference,
                Status = status,
                AvailabilityStatus = availabilityStatus,
                LastUpdatedBy = lastUpdatedBy
            };
        }).Single();
        
        var itemResponse = await FindBothBookingAndIndexTimestampedWithRetryAsync(bookingDetails);

        await VerifyBookingAuditLog(itemResponse.Item1);
        await VerifyBookingIndexAuditLog(itemResponse.Item2);
    }

    private async Task VerifyBookingAuditLog(BookingTimestamped bookingTimestamped)
    {
        var siteId = GetSiteId();
        var fileName = AuditBlobHelper.GetBlobName(
            bookingTimestamped.DocumentType,
            bookingTimestamped.CosmosTimestamp,
            bookingTimestamped.Id + "-" + bookingTimestamped.Site
        );
        var auditJson = await AuditBlobHelper.PollForAuditLogAsync(
            BookingContainer,
            fileName
        );

        auditJson.Should().NotBeNullOrEmpty($"The booking audit log for site {siteId} was not found or was empty.");

        var auditDoc = JsonConvert.DeserializeObject<BookingDocument>(auditJson!, new TimeOnlyConverter());

        auditDoc.Should().BeEquivalentTo(bookingTimestamped, options => options
            .Excluding(ctx => ctx.CosmosTimestamp)
            .Excluding(ctx => ctx.RawTimestamp)
            .Excluding(x => x.AttendeeDetails)
            .Excluding(x => x.ContactDetails)
        );
        bookingTimestamped.ContactDetails.Should().NotBeNull("because Cosmos should store the contact info");
        auditDoc.ContactDetails.Should().BeNull("because the Audit record should omit contact info");
        auditDoc.AttendeeDetails.FirstName.Should().BeNull();
        auditDoc.AttendeeDetails.LastName.Should().BeNull();
        auditDoc.AttendeeDetails.DateOfBirth.Should().Be(DateOnly.MinValue);
    }

    private async Task VerifyBookingIndexAuditLog(BookingIndexTimestamped bookingIndexTimestamped)
    {
        var identifier = bookingIndexTimestamped.Id + "-booking_index";
        var fileName = AuditBlobHelper.GetBlobName(
            bookingIndexTimestamped.DocumentType,
            bookingIndexTimestamped.CosmosTimestamp,
            identifier
        );

        var auditJson = await AuditBlobHelper.PollForAuditLogAsync("index_data", fileName);
        auditJson.Should().NotBeNullOrEmpty($"Index audit log {fileName} not found.");

        var auditDoc = JsonConvert.DeserializeObject<BookingIndexDocument>(auditJson!);

        auditDoc.Should().BeEquivalentTo(bookingIndexTimestamped, options => options
            .Excluding(ctx => ctx.CosmosTimestamp)
            .Excluding(ctx => ctx.RawTimestamp)
        );
    }
    

    [And("the site with details should be audited in blob storage")]
    public async Task AssertSiteAudited(DataTable dataTable)
    {
        var expectedSite = dataTable.Rows.Skip(1).Take(1).Select(row => new SiteDocument
        {
            Id = GetSiteId(),
            Name = dataTable.GetRowValueOrDefault(row, "Name"),
            Address = dataTable.GetRowValueOrDefault(row, "Address"),
            PhoneNumber = dataTable.GetRowValueOrDefault(row, "PhoneNumber"),
            OdsCode = dataTable.GetRowValueOrDefault(row, "OdsCode"),
            Region = dataTable.GetRowValueOrDefault(row, "Region"),
            IntegratedCareBoard = dataTable.GetRowValueOrDefault(row, "ICB"),
            InformationForCitizens = dataTable.GetRowValueOrDefault(row, "InformationForCitizens"),
            DocumentType = "site",
            Accessibilities = ParseAccessibilities(dataTable.GetRowValueOrDefault(row, "Accessibilities")),
            Location = new Core.Sites.Location("Point",
            [
                dataTable.GetDoubleRowValueOrDefault(row, "Longitude", -60d),
                    dataTable.GetDoubleRowValueOrDefault(row, "Latitude", -60d)
            ]),
            Type = dataTable.GetRowValueOrDefault(row, "Type"),
            IsDeleted = dataTable.GetBoolRowValueOrDefault(row, "IsDeleted"),
            Status = dataTable.GetEnumRowValueOrDefault<SiteStatus>(row, "Status"),
            LastUpdatedBy = dataTable.GetRowValueOrDefault(row, "Last Updated By") == _userId ? _userId : CreateUniqueTestValue(dataTable.GetRowValueOrDefault(row, "Last Updated By"))
        }).Single();
        
        var itemResponse = await FindSiteTimestampedWithRetryAsync(expectedSite);
        
        var fileName = AuditBlobHelper.GetBlobName(
            itemResponse.DocumentType,
            itemResponse.CosmosTimestamp,
            itemResponse.Id + "-" + itemResponse.DocumentType
        );

        var siteJson = await AuditBlobHelper.PollForAuditLogAsync(
            CoreContainer,
            fileName
        );

        siteJson.Should()
            .NotBeNull($"Audit log for site {expectedSite.Id} was not found in blob storage within the timeout period.");

        var siteDoc = JsonConvert.DeserializeObject<SiteDocument>(siteJson!);

        siteDoc.Should().BeEquivalentTo(itemResponse, options => options
            .Excluding(ctx => ctx.CosmosTimestamp)
            .Excluding(ctx => ctx.RawTimestamp)
        );
    }
    
    [And(@"a '(.+)' user should be audited in blob storage")]
    public async Task VerifyAuditLogExists(string user)
    {
        var itemResponse = await CosmosReadItem<UserTimestamped>(CoreContainer, GetUserId(user), new PartitionKey("user"),
            CancellationToken.None);

        var fileName = AuditBlobHelper.GetBlobName(
            itemResponse.Resource.DocumentType,
            itemResponse.Resource.CosmosTimestamp,
            itemResponse.Resource.Id + "-" + itemResponse.Resource.DocumentType
        );

        var userJson = await AuditBlobHelper.PollForAuditLogAsync(
            CoreContainer,
            fileName
        );

        userJson.Should()
            .NotBeNull($"Audit log for {user} was not found in blob storage within the timeout period.");

        var userDoc = JsonConvert.DeserializeObject<UserDocument>(userJson!);

        userDoc.Should().BeEquivalentTo(itemResponse.Resource, options => options
            .Excluding(ctx => ctx.CosmosTimestamp)
            .Excluding(ctx => ctx.RawTimestamp)
        );
    }

    [And(@"the availability with details at the default site should be audited in blob storage")]
    public async Task VerifySingleSessionAvailabilityAudited(DataTable expectedDailyAvailabilityTable)
    {
        var site = GetSiteId();
        var expectedDocument = DailyAvailabilityDocumentsFromTable(site, expectedDailyAvailabilityTable).Single();
        
        var itemResponse = await FindAvailabilityTimestampedWithRetryAsync(expectedDocument);

        var fileName = AuditBlobHelper.GetBlobName(
            itemResponse.DocumentType,
            itemResponse.CosmosTimestamp,
            itemResponse.Id + "-" + itemResponse.Site
        );

        var auditJson = await AuditBlobHelper.PollForAuditLogAsync(
            BookingContainer,
            fileName
        );

        auditJson.Should()
            .NotBeNullOrEmpty($"The daily availability audit log for site {site} was not found or was empty.");

        var auditDoc = JsonConvert.DeserializeObject<DailyAvailabilityDocument>(auditJson!, new TimeOnlyConverter());

        auditDoc.Should().BeEquivalentTo(itemResponse, options => options
            .Excluding(ctx => ctx.CosmosTimestamp)
            .Excluding(ctx => ctx.RawTimestamp)
        );
    }
    
    [And(@"the availability on '(.+)' with no sessions at the default site should be audited in blob storage")]
    public async Task VerifyNoSessionsAvailabilityAudited(string naturalDate)
    {
        var site = GetSiteId();
        var expectedDocument = new DailyAvailabilityDocument
        {
            Id = NaturalLanguageDate.Parse(naturalDate).ToString("yyyyMMdd"),
            Date = NaturalLanguageDate.Parse(naturalDate),
            Site = site,
            DocumentType = "daily_availability",
            Sessions = new List<Session>().ToArray(),
            LastUpdatedBy = _userId
        };
        
        var itemResponse = await FindAvailabilityTimestampedWithRetryAsync(expectedDocument);

        var fileName = AuditBlobHelper.GetBlobName(
            itemResponse.DocumentType,
            itemResponse.CosmosTimestamp,
            itemResponse.Id + "-" + itemResponse.Site
        );

        var auditJson = await AuditBlobHelper.PollForAuditLogAsync(
            BookingContainer,
            fileName
        );

        auditJson.Should()
            .NotBeNullOrEmpty($"The daily availability audit log for site {site} was not found or was empty.");

        var auditDoc = JsonConvert.DeserializeObject<DailyAvailabilityDocument>(auditJson!, new TimeOnlyConverter());

        auditDoc.Should().BeEquivalentTo(itemResponse, options => options
            .Excluding(ctx => ctx.CosmosTimestamp)
            .Excluding(ctx => ctx.RawTimestamp)
        );
    }

    [Then(@"a '(.+)' notification should be audited for user '(.+)' in blob storage")]
    [And(@"a '(.+)' notification should be audited for user '(.+)' in blob storage")]
    public async Task VerifyNotificationAuditLog(string notificationType, string user)
    {
        var auditNotificationTimestamped = await FindAuditNotificationTimestampedWithRetryAsync(notificationType, user);
        var fileName = AuditBlobHelper.GetBlobName(
            auditNotificationTimestamped.DocumentType,
            auditNotificationTimestamped.CosmosTimestamp,
            auditNotificationTimestamped.Id
        );
        var auditJson = await AuditBlobHelper.PollForAuditLogAsync(
            AuditContainer,
            fileName
        );

        auditJson.Should().NotBeNullOrEmpty("The audit log was not found or was empty.");

        var auditDoc = JsonConvert.DeserializeObject<AuditNotificationDocument>(auditJson!);

        auditDoc.Should().BeEquivalentTo(auditNotificationTimestamped, options => options
            .Excluding(ctx => ctx.CosmosTimestamp)
            .Excluding(ctx => ctx.RawTimestamp)
        );
    }

    private async Task<AuditNotificationTimestamped> FindAuditNotificationTimestampedWithRetryAsync(
        string notificationType, string user)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < _pollingTimeout)
        {
            var notifications = (await CosmosQueryFeed<AuditNotificationTimestamped>(AuditContainer,
                d => d.DestinationId == GetUserId(user) && d.NotificationName == notificationType)).ToList();

            if (notifications.Count > 0)
            {
                //_testId unique per run
                return notifications.Single();
            }

            await Task.Delay(_pollingInterval); // Wait before retrying
        }

        throw new TimeoutException("AuditNotificationDocument not found within the timeout period.");
    }

    private async Task<AuditFunctionDocumentTimestamped> FindAuditFunctionTimestampedWithRetryAsync(string user,
        string functionName, string siteId)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < _pollingTimeout)
        {
            var documentsFound = (await CosmosQueryFeed<AuditFunctionDocumentTimestamped>(AuditContainer,
                d => d.User == user && d.FunctionName == functionName && d.Site == siteId)).ToList();

            if (documentsFound.Count > 0)
            {
                // Audit logs with no site will not be unique on test reruns, so there may be duplicate on the 2nd run
                if (siteId is not null)
                {
                    return documentsFound.Single();
                }

                return documentsFound.OrderByDescending(doc => doc.Timestamp).First();
            }

            await Task.Delay(_pollingInterval); // Wait before retrying
        }

        throw new TimeoutException("AuditFunctionDocument not found within the timeout period.");
    }

    private async Task<(BookingTimestamped, BookingIndexTimestamped)> FindBothBookingAndIndexTimestampedWithRetryAsync(BookingDocument details)
    {
        var startTime = DateTime.UtcNow;

        var siteId = GetSiteId();
        
        while (DateTime.UtcNow - startTime < _pollingTimeout)
        {
            //make sure find the right version of the document
            var bookingDocumentsFound = (await CosmosQueryFeed<BookingTimestamped>("booking_data",
                d =>
                    d.Id == details.Reference &&
                    d.Site == siteId &&
                    d.Status.ToString() == details.Status.ToString() &&
                    d.AvailabilityStatus.ToString() == details.AvailabilityStatus.ToString() &&
                    d.LastUpdatedBy == details.LastUpdatedBy
            )).ToList();
            
            //make sure find the right version of the document
            var indexDocumentsFound = (await CosmosQueryFeed<BookingIndexTimestamped>("index_data",
                d =>
                    d.Id == details.Reference &&
                    d.DocumentType == "booking_index" &&
                    d.Status.ToString() == details.Status.ToString()
            )).ToList();

            if (bookingDocumentsFound.Count > 0 && indexDocumentsFound.Count > 0)
            {
                return (bookingDocumentsFound.Single(), indexDocumentsFound.Single());
            }

            await Task.Delay(_pollingInterval); // Wait before retrying
        }

        throw new TimeoutException("BookingDocument or IndexDocument not found within the timeout period.");
    }
    
    private async Task<SiteTimestamped> FindSiteTimestampedWithRetryAsync(SiteDocument details)
    {
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < _pollingTimeout)
        {
            //make sure find the right version of the document
            var siteDocumentsFound = (await CosmosQueryFeed<SiteTimestamped>(CoreContainer,
                d =>
                    d.Id == details.Id &&
                    d.Address == details.Address &&
                    d.OdsCode == details.OdsCode &&
                    d.IntegratedCareBoard == details.IntegratedCareBoard &&
                    d.Name == details.Name &&
                    d.PhoneNumber == details.PhoneNumber &&
                    d.Region == details.Region &&
                    d.Type == details.Type
                    // d.LastUpdatedBy == details.LastUpdatedBy
            )).ToList();

            if (siteDocumentsFound.Count > 0)
            {
                return siteDocumentsFound.Single();
            }

            await Task.Delay(_pollingInterval); // Wait before retrying
        }

        throw new TimeoutException("SiteDocument not found within the timeout period.");
    }
    
    private async Task<DailyAvailabilityTimestamped> FindAvailabilityTimestampedWithRetryAsync(DailyAvailabilityDocument details)
    {
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < _pollingTimeout)
        {
            List<DailyAvailabilityTimestamped> availabilityDocumentsFound;
            if (details.Sessions.Length > 0)
            {
                //single session with a single service assertion for now!!
                
                //make sure find the right version of the document
                availabilityDocumentsFound = (await CosmosQueryFeed<DailyAvailabilityTimestamped>("booking_data",
                    d =>
                        d.Id == details.Id &&
                        d.Site == details.Site &&
                        d.Date == details.Date &&
                        d.Sessions[0].Services[0] == details.Sessions[0].Services[0] &&
                        d.Sessions[0].Capacity == details.Sessions[0].Capacity &&
                        d.Sessions[0].SlotLength == details.Sessions[0].SlotLength &&
                        d.Sessions[0].From.ToString() == details.Sessions[0].From.ToString() &&
                        d.Sessions[0].Until.ToString() == details.Sessions[0].Until.ToString() &&
                        d.LastUpdatedBy == details.LastUpdatedBy
                )).ToList();
            }
            else
            {
                //make sure find the right version of the document
                availabilityDocumentsFound = (await CosmosQueryFeed<DailyAvailabilityTimestamped>("booking_data",
                    d =>
                        d.Id == details.Id &&
                        d.Site == details.Site &&
                        d.Date == details.Date &&
                        // ReSharper disable once UseMethodAny.2
                        //have to use .Count instead of .Length/.Any!
                        d.Sessions.Count() == 0 &&
                        d.LastUpdatedBy == details.LastUpdatedBy
                )).ToList();
            }

            if (availabilityDocumentsFound.Count > 0)
            {
                return availabilityDocumentsFound.Single();
            }

            await Task.Delay(_pollingInterval); // Wait before retrying
        }

        throw new TimeoutException("DailyAvailabilityDocument not found within the timeout period.");
    }
}
