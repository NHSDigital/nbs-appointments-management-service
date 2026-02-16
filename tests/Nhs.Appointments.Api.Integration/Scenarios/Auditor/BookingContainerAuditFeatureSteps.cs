using FluentAssertions;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Persistance.Models;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace Nhs.Appointments.Api.Integration.Scenarios.Auditor;

[FeatureFile("./Scenarios/Auditor/BookingContainerAudit.feature")]
public sealed class BookingContainerAuditFeatureSteps : BaseFeatureSteps
{
    private readonly AuditHelper _auditHelper;
    private DailyAvailabilityDocument _createdAvailability;
    private const string containerName = "booking_data";
    private HttpResponseMessage Response;
    private string _createdBookingReference;

    public BookingContainerAuditFeatureSteps()
    {
        _auditHelper = new AuditHelper();
    }

    [Given("there is no existing availability for a created default site")]
    public async Task NoAvailability()
    {
        await SetupSite(GetSiteId());
    }

    [When("I apply the following availability template")]
    public async Task ApplyTemplate(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;
        var fromDate = NaturalLanguageDate.Parse(cells.ElementAt(0).Value);
        var untilDate = NaturalLanguageDate.Parse(cells.ElementAt(1).Value);
        var days = DeriveWeekDaysInRange(fromDate, untilDate);

        var request = new
        {
            site = GetSiteId(),
            from = fromDate,
            until = untilDate,
            template = new
            {
                days = ParseDays(days),
                sessions = new[]
                {
                    new
                    {
                        from = cells.ElementAt(3).Value,
                        until = cells.ElementAt(4).Value,
                        slotLength = int.Parse(cells.ElementAt(5).Value),
                        capacity = int.Parse(cells.ElementAt(6).Value),
                        services = cells.ElementAt(7).Value.Split(',').Select(s => s.Trim()).ToArray(),
                    }
                }
            },
            mode = cells.ElementAt(8).Value
        };

        var payload = JsonResponseWriter.Serialize(request);
        _response = await GetHttpClientForTest().PostAsync("http://localhost:7071/api/availability/apply-template",
            new StringContent(payload));
        _statusCode = _response.StatusCode;
    }

    [Then("the request is successful and the following daily availability sessions are created")]
    public async Task AssertDailyAvailability(DataTable expectedDailyAvailabilityTable)
    {
        _statusCode.Should().Be(HttpStatusCode.OK);
        var site = GetSiteId();
        var expectedDocuments = DailyAvailabilityDocumentsFromTable(site, expectedDailyAvailabilityTable);
        var container = Client.GetContainer("appts", "booking_data");
        var actualDocuments = await RunQueryAsync<DailyAvailabilityDocument>(container,
            d => d.DocumentType == "daily_availability" && d.Site == site);
        _createdAvailability = actualDocuments.First();
        actualDocuments.Count().Should().Be(expectedDocuments.Count());
        actualDocuments.Should().BeEquivalentTo(expectedDocuments);
    }


    [And(@"the availability audit log in StorageAccount should match the Cosmos DB record")]
    public async Task VerifySiteAuditLogMatchesCosmos()
    {
        var (cosmosDoc, timeStamp) = await GetCosmosDailyAvailability();
        var fileName = _auditHelper.GetBlobName(
            cosmosDoc.DocumentType, 
            timeStamp, 
            cosmosDoc.Id + "-" + cosmosDoc.Site
        );
        var auditJson = await _auditHelper.PollForAuditLogAsync(
            containerName,
            fileName
        );

        auditJson.Should().NotBeNullOrEmpty($"The daily availability audit log for site {GetSiteId()} was not found or was empty.");

        var auditDoc = JsonConvert.DeserializeObject<DailyAvailabilityDocument>(auditJson, new TimeOnlyConverter());

        auditDoc.Should().BeEquivalentTo(cosmosDoc, options => options
            .Excluding(ctx => ctx.Path.Contains("_etag"))
            .Excluding(ctx => ctx.Path.Contains("_ts"))
            .Excluding(ctx => ctx.Path.Contains("_rid"))
            .Excluding(ctx => ctx.Path.Contains("_self"))
            .Excluding(ctx => ctx.Path.Contains("_attachments"))
            .Excluding(ctx => ctx.Path.Contains("_lsn"))
        );
    }

    [When("I cancel the following session")]
    public async Task CancelDailyAvailability(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.ToList();
            var date = DateTime.ParseExact(
                NaturalLanguageDate.Parse(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"),
                "yyyy-MM-dd", null);

            object payload = new
            {
                site = GetSiteId(),
                from = DateOnly.FromDateTime(date),
                to = DateOnly.FromDateTime(date),
                sessionMatcher = new
                {
                    from = cells.ElementAt(1).Value,
                    until = cells.ElementAt(2).Value,
                    services = cells.ElementAt(3).Value.Split(',').Select(s => s.Trim()).ToArray(),
                    slotLength = int.Parse(cells.ElementAt(4).Value),
                    capacity = int.Parse(cells.ElementAt(5).Value)
                },
                sessionReplacement = null as Session
            };

            _response = await GetHttpClientForTest().PostAsJsonAsync("http://localhost:7071/api/session/edit", payload);
        }
    }

    [When("I make a provisional appointment with the following details")]
    public async Task MakeProvisionalBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        object payload = new
        {
            from =
                DateTime.ParseExact(
                    $"{NaturalLanguageDate.Parse(cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {cells.ElementAt(1).Value}",
                    "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm"),
            duration = cells.ElementAt(2).Value,
            service = cells.ElementAt(3).Value,
            site = GetSiteId(),
            kind = "provisional",
            attendeeDetails = new
            {
                nhsNumber = EvaluateNhsNumber(cells.ElementAt(4).Value),
                firstName = cells.ElementAt(5).Value,
                lastName = cells.ElementAt(6).Value,
                dateOfBirth = cells.ElementAt(7).Value
            },
            contactDetails =
                new[]
                {
                    new { type = "email", value = cells.ElementAt(8).Value },
                    new { type = "phone", value = cells.ElementAt(9).Value },
                },
        };

        Response = await GetHttpClientForTest().PostAsJsonAsync("http://localhost:7071/api/booking", payload);
        var result = JsonConvert.DeserializeObject<MakeBookingResponse>(await Response.Content.ReadAsStringAsync());
        _createdBookingReference = result.BookingReference;
    }

    [And(@"a sanitized version of the booking should be audited in StorageAccount")]
    public async Task VerifyBookingAuditLogMatchesCosmos()
    {
        var (cosmosDoc, timeStamp) = await GetCosmosBooking();
        var fileName = _auditHelper.GetBlobName(
            cosmosDoc.DocumentType,
            timeStamp,
            cosmosDoc.Id + "-" + cosmosDoc.Site
        );
        var auditJson = await _auditHelper.PollForAuditLogAsync(
            containerName,
            fileName
        );

        auditJson.Should().NotBeNullOrEmpty($"The booking audit log for site {GetSiteId()} was not found or was empty.");

        var auditDoc = JsonConvert.DeserializeObject<BookingDocument>(auditJson, new TimeOnlyConverter());

        auditDoc.Should().BeEquivalentTo(cosmosDoc, options => options
            .Excluding(ctx => ctx.Path.Contains("_"))
            .Excluding(x => x.ContactDetails)
        );
        cosmosDoc.ContactDetails.Should().NotBeNull("because Cosmos should store the contact info");
        auditDoc.ContactDetails.Should().BeNull("because the Audit record should mask or omit contact info");
    }

    [And("I confirm the rescheduled booking")]
    public async Task ConfirmBooking()
    {
        var url = $"http://localhost:7071/api/booking/{_createdBookingReference}/confirm";

        _response = await GetHttpClientForTest().PostAsync(url, null);
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then("the rescheduled booking is no longer marked as provisional")]
    public async Task AssertRescheduledBookingIsNotProvisional()
    {
        var siteId = GetSiteId();
        var bookingReference = _createdBookingReference;
        var actualBooking = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        actualBooking.Resource.Status.Should().Be(AppointmentStatus.Booked);

        var actualBookingIndex = await Client.GetContainer("appts", "index_data")
            .ReadItemAsync<BookingIndexDocument>(bookingReference, new PartitionKey("booking_index"));
        actualBookingIndex.Resource.Status.Should().Be(AppointmentStatus.Booked);
    }

    [And(@"the booking index should be audited in StorageAccount")]
    public async Task VerifyBookingIndexAuditLog()
    {
        var (cosmosIndex, timeStamp) = await GetCosmosBookingIndex();

        var fileName = _auditHelper.GetBlobName(
            cosmosIndex.DocumentType, 
            timeStamp,
            cosmosIndex.Id + "-booking_index" 
        );

        var auditJson = await _auditHelper.PollForAuditLogAsync("index_data", fileName);
        auditJson.Should().NotBeNullOrEmpty($"Index audit log {fileName} not found.");

        var auditDoc = JsonConvert.DeserializeObject<BookingIndexDocument>(auditJson);

        auditDoc.Should().BeEquivalentTo(cosmosIndex, options => options
            .Excluding(ctx => ctx.Path.Contains("_"))
        );
    }

    private async Task<(DailyAvailabilityDocument Document, DateTimeOffset Timestamp)> GetCosmosDailyAvailability()
    {
        var container = Client.GetContainer("appts", containerName);
        var siteId = GetSiteId();

        var deserializedResponse = await container.ReadItemAsync<CosmosDocumentTestWrapper<DailyAvailabilityDocument>>(
            _createdAvailability.Id, 
            new PartitionKey(siteId)
        );

        var wrapper = deserializedResponse.Resource;

        return (wrapper.Document, wrapper.Timestamp);
    }

    private async Task<(BookingDocument Document, DateTimeOffset Timestamp)> GetCosmosBooking()
    {
        var container = Client.GetContainer("appts", containerName);
        var createBookingResponse = JsonConvert.DeserializeObject<MakeBookingResponse>(await Response.Content.ReadAsStringAsync());

        var bookingResponse = await container.ReadItemAsync<CosmosDocumentTestWrapper<BookingDocument>>(
            createBookingResponse.BookingReference,
            new PartitionKey(GetSiteId())
        );

        var wrapper = bookingResponse.Resource;

        return (wrapper.Document, wrapper.Timestamp);
    }

    private async Task<(BookingIndexDocument Document, DateTimeOffset Timestamp)> GetCosmosBookingIndex()
    {
        var container = Client.GetContainer("appts", "index_data");
        var bookingReference = _createdBookingReference;

        var response = await container.ReadItemAsync<CosmosDocumentTestWrapper<BookingIndexDocument>>(
            bookingReference,
            new PartitionKey("booking_index")
        );

        return (response.Resource.Document, response.Resource.Timestamp);
    }
}
