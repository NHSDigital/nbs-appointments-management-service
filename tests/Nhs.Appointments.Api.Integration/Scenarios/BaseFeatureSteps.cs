using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Json;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;
using Feature = Xunit.Gherkin.Quick.Feature;
using Location = Nhs.Appointments.Core.Sites.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract partial class BaseFeatureSteps : Feature
{
    public enum BookingType { Recent, Confirmed, Provisional, ExpiredProvisional, Orphaned, Cancelled }

    protected const string DefaultSiteId = "beeae4e0-dd4a-4e3a-8f4d-738f9418fb51";
    
    protected const string ApiSigningKey =
        "2EitbEouxHQ0WerOy3TwcYxh3/wZA0LaGrU1xpKg0KJ352H/mK0fbPtXod0T0UCrgRHyVjF6JfQm/LillEZyEA==";

    private const string AppointmentsApiUrl = "http://localhost:7071/api";

    /// <summary>
    /// A long lat that does not interfere with site location tests
    /// </summary>
    protected Location OutOfTheWayLocation => new("Point", [-60d, -60d]);

    protected readonly Guid _testId = Guid.NewGuid();
    protected readonly CosmosClient Client;
    protected readonly HttpClient Http;
    protected readonly Mapper Mapper;
    protected List<BookingDocument> MadeBookings = new List<BookingDocument>();
    protected readonly Dictionary<int, string> _bookingReferences = new();
    protected readonly string _userId = "api@test";
    protected HttpResponseMessage _response { get; set; }

    public BaseFeatureSteps()
    {
        CosmosClientOptions options = new()
        {
            HttpClientFactory = () => new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }),
            Serializer = new CosmosJsonSerializer(),
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true
        };

        var requestSigner = new RequestSigningHandler(new RequestSigner(TimeProvider.System, ApiSigningKey));
        requestSigner.InnerHandler = new HttpClientHandler();
        Http = new HttpClient(requestSigner);
        Http.DefaultRequestHeaders.Add("ClientId", "test");

        Client = new(
            accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT") ?? "https://localhost:8081/",
            authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_TOKEN") ??
                                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            clientOptions: options);

        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CosmosAutoMapperProfile>();
        });
        Mapper = new Mapper(mapperConfiguration);
    }
    
    /// <summary>
    /// Setting up test data for scenarios can sometimes overload CosmosDB
    /// Add a backoff retry if the data isn't written
    /// </summary>
    protected async Task CosmosAction_RetryOnTooManyRequests<T>(
        CosmosAction cosmosAction,
        Container container, 
        T item,
        PartitionKey? partitionKey = null,
        ItemRequestOptions requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var maxRetries = 5;
        var delay = 100;
        var backoffFactor = 2;

        var attempt = 1; 
        
        while (attempt <= maxRetries)
        {
            try
            {
                switch (cosmosAction)
                {
                    case CosmosAction.Create:
                        await container.CreateItemAsync(item, partitionKey, requestOptions, cancellationToken);
                        return;
                    case CosmosAction.Upsert:
                        await container.UpsertItemAsync(item, partitionKey, requestOptions, cancellationToken);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cosmosAction), cosmosAction, null);
                }
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(delay, cancellationToken);
                    delay *= backoffFactor;
                    attempt++;
                }
                else
                {
                    throw;
                }
            }
        }
    }

    protected string NhsNumber { get; private set; } = CreateRandomTenCharacterString();

    protected string GetTestId => $"{_testId}";

    public BookingReferenceManager BookingReferences { get; } = new();

    protected string GetContactInfo(ContactItemType type) => type switch
    {
        ContactItemType.Landline => $"0113{NhsNumber.Substring(0, 7)}",
        ContactItemType.Email => $"{NhsNumber}@test.com",
        ContactItemType.Phone => $"07777{NhsNumber.Substring(0, 6)}",
        _ => throw new ArgumentOutOfRangeException()
    };

    [Given("the site is configured for MYA")]
    public async Task SetupSite()
    {
        var site = new SiteDocument
        {
            Id = GetSiteId(),
            DocumentType = "site",
            Location = OutOfTheWayLocation
        };
        await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "core_data"), site);
    }

    // TODO: Added for BulkImport tests as it requires a valid Guid
    // To clean up this method & the one above so all siteId's use only a Guid
    [Given("a new site is configured for MYA")]
    public async Task SetupNewSite()
    {
        var site = new SiteDocument
        {
            Id = _testId.ToString(),
            Name = "Test Site",
            DocumentType = "site",
            OdsCode = "ODS1",
            IntegratedCareBoard = "ICB1",
            Region = "R1",
            Location = OutOfTheWayLocation
        };
        await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "core_data"), site);
    }

    protected async Task SetLocalFeatureToggleOverride(string name, string state)
    {
        var response = await Http.PatchAsync($"{AppointmentsApiUrl}/feature-flag-override/{name}?enabled={state}",
            null);

        response.EnsureSuccessStatusCode();
    }
    
    [Given("the following sessions exist for existing site '(.+)'")]
    [And("the following sessions exist for existing site '(.+)'")]
    public async Task SetupSessionsForSite(string site, DataTable dataTable)
    {
        await SetupSessions(site, dataTable);
    }

    [Given("the following sessions exist for a created default site")]
    [And("the following sessions exist for a created default site")]
    public async Task SetupSessions(DataTable dataTable)
    {
        await SetupSite(GetSiteId());
        await SetupSessions(DefaultSiteId, dataTable);
    }

    protected async Task SetupSite(string siteId)
    {
        var site = new SiteDocument
        {
            Id = siteId,
            DocumentType = "site",
            Location = OutOfTheWayLocation
        };
        await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "core_data"), site);
    }

    public async Task SetupSessions(string siteDesignation, DataTable dataTable)
    {
        var site = GetSiteId(siteDesignation);
        var availabilityDocuments = DailyAvailabilityDocumentsFromTable(site, dataTable).ToList();
        foreach (var document in availabilityDocuments)
        {
            await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "booking_data"), document);
        }
    }

    [And(@"a citizen with the NHS Number '(\d+)'")]
    public void SetNhsNumber(string nhsNumber)
    {
        NhsNumber = nhsNumber;
    }

    protected string EvaluateNhsNumber(string nhsNumber)
    {
        return nhsNumber == "*" ? NhsNumber : nhsNumber;
    }

    protected IEnumerable<DailyAvailabilityDocument> DailyAvailabilityDocumentsFromTable(string site,
        DataTable dataTable)
    {
        var sessions = dataTable.Rows.Skip(1).Select((row, index) => new DailyAvailabilityDocument
        {
            Id = NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value).ToString("yyyyMMdd"),
            Date = NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value),
            Site = site,
            DocumentType = "daily_availability",
            Sessions = new[]
            {
                new Session
                {
                    From = TimeOnly.Parse(row.Cells.ElementAt(1).Value),
                    Until = TimeOnly.Parse(row.Cells.ElementAt(2).Value),
                    Services = row.Cells.ElementAt(3).Value.Split(",").Select(x => x.Trim()).ToArray(),
                    Capacity = int.Parse(row.Cells.ElementAt(5).Value),
                    SlotLength = int.Parse(row.Cells.ElementAt(4).Value)
                }
            }
        });

        return sessions.GroupBy(s => s.Date).Select(g => new DailyAvailabilityDocument
        {
            Id = g.First().Id,
            Date = g.Key,
            Site = g.First().Site,
            DocumentType = "daily_availability",
            Sessions = g.SelectMany(s => s.Sessions).ToArray()
        });
    }

    protected IEnumerable<DailyAvailabilityDocument> DailyAvailabilityDocumentsFromTable(string site,
    DataTable dataTable, bool lastUpdatedByEnabled, string flag)
    {
        var sessions = dataTable.Rows.Skip(1).Select((row, index) => new DailyAvailabilityDocument
        {
            Id = NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value).ToString("yyyyMMdd"),
            Date = NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value),
            Site = site,
            DocumentType = "daily_availability",
            Sessions = new[]
            {
                new Session
                {
                    From = TimeOnly.Parse(row.Cells.ElementAt(1).Value),
                    Until = TimeOnly.Parse(row.Cells.ElementAt(2).Value),
                    Services = row.Cells.ElementAt(3).Value.Split(",").Select(x => x.Trim()).ToArray(),
                    Capacity = int.Parse(row.Cells.ElementAt(5).Value),
                    SlotLength = int.Parse(row.Cells.ElementAt(4).Value)
                }
            },
            LastUpdatedBy = flag == Flags.AuditLastUpdatedBy && lastUpdatedByEnabled ? _userId : null
        });

        return sessions.GroupBy(s => s.Date).Select(g => new DailyAvailabilityDocument
        {
            Id = g.First().Id,
            Date = g.Key,
            Site = g.First().Site,
            DocumentType = "daily_availability",
            Sessions = g.SelectMany(s => s.Sessions).ToArray(),
            LastUpdatedBy = g.First().LastUpdatedBy
        });
    }

    protected string DeriveWeekDaysInRange(DateOnly startDate, DateOnly? endDate)
    {
        if (endDate is null)
        {
            return string.Empty;
        }

        if (startDate.AddDays(7) <= endDate)
        {
            return "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";
        }

        var days = new List<DayOfWeek>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            days.Add(date.DayOfWeek);
        }

        return string.Join(",", days);
    }

    [Given("the following bookings have been made")]
    [And("the following bookings have been made")]
    public async Task SetupBookings(DataTable dataTable)
    {
        await SetupBookings(DefaultSiteId, dataTable, BookingType.Confirmed);
    }
    
    [Given("the following bookings have been made for site '(.+)'")]
    [And("the following bookings have been made for site '(.+)'")]
    public async Task SetupBookingsForSite(string site, DataTable dataTable)
    {
        await SetupBookings(site, dataTable, BookingType.Confirmed);
    }

    [Given("the following recent bookings have been made")]
    [And("the following recent bookings have been made")]
    public async Task SetupRecentBookings(DataTable dataTable)
    {
        await SetupBookings(DefaultSiteId, dataTable, BookingType.Recent);
    }

    [Given("the following provisional bookings have been made")]
    [And("the following provisional bookings have been made")]
    public async Task SetupProvisionalBookings(DataTable dataTable)
    {
        await SetupBookings(DefaultSiteId, dataTable, BookingType.Provisional);
    }

    [Given("the following expired provisional bookings have been made")]
    [And("the following expired provisional bookings have been made")]
    public async Task SetupExpiredProvisionalBookings(DataTable dataTable)
    {
        await SetupBookings(DefaultSiteId, dataTable, BookingType.ExpiredProvisional);
    }

    [Given("the following cancelled bookings have been made")]
    [And("the following cancelled bookings have been made")]
    public async Task SetupCancelledBookings(DataTable dataTable) =>
        await SetupBookings(DefaultSiteId, dataTable, BookingType.Cancelled);

    [And("the following orphaned bookings exist")]
    public async Task SetupOrphanedBookings(DataTable dataTable) =>
        await SetupBookings(DefaultSiteId, dataTable, BookingType.Orphaned);
    
    [And("the following orphaned bookings exist for site '(.+)'")]
    public async Task SetupOrphanedBookingsForSite(string site, DataTable dataTable) =>
        await SetupBookings(site, dataTable, BookingType.Orphaned);

    protected DateTime ParseDayAndTime(string day, string time) => DateTime.ParseExact(
        $"{NaturalLanguageDate.Parse(day).ToString("yyyy-MM-dd")} {time}",
        "yyyy-MM-dd HH:mm", null);

    protected IEnumerable<(BookingDocument booking, BookingIndexDocument bookingIndex)>
        BuildBookingAndIndexDocumentsFromDataTable(
            DataTable dataTable)
    {
        return dataTable.Rows.Skip(1).Select((row, index) =>
        {
            var bookingType = dataTable.GetEnumRowValue(row, "Booking Type", BookingType.Confirmed);
            var reference = CreateCustomBookingReference(dataTable.GetRowValueOrDefault(row, "Reference")) ??
                            BookingReferences.GetBookingReference(index, bookingType);
            var site = GetSiteId(dataTable.GetRowValueOrDefault(row, "Site", DefaultSiteId));
            var service = dataTable.GetRowValueOrDefault(row, "Service", "RSV:Adult");
            var status = dataTable.GetEnumRowValueOrDefault<AppointmentStatus>(row, "Status") ?? MapStatus(bookingType);

            var day = dataTable.GetRowValueOrDefault(row, "Date", "Tomorrow");
            var time = dataTable.GetRowValueOrDefault(row, "Time", "10:00");
            var from = ParseDayAndTime(day, time);

            var createdDay = dataTable.GetRowValueOrDefault(row, "Created Day");
            var createdTime = dataTable.GetRowValueOrDefault(row, "Created Time");
            var created = createdDay != null && createdTime != null
                ? ParseDayAndTime(createdDay, createdTime)
                : GetCreationDateTime(bookingType);

            var duration = int.Parse(dataTable.GetRowValueOrDefault(row, "Duration", "10"));
            var availabilityStatus =
                dataTable.GetEnumRowValue(row, "Availability Status", MapAvailabilityStatus(bookingType));

            var cancellationReason = dataTable.GetEnumRowValueOrDefault<CancellationReason>(row, "Cancellation Reason");
            var cancellationNotificationStatus =
                dataTable.GetEnumRowValueOrDefault<CancellationNotificationStatus>(row,
                    "Cancellation Notification Status");

            var nhsNumber = dataTable.GetRowValueOrDefault(row, "Nhs Number", NhsNumber);

            var additionalData = BuildAdditionalDataFromDataTable(dataTable, row);

            var booking = new BookingDocument
            {
                Id = reference,
                DocumentType = "booking",
                Reference = reference,
                From = from,
                Duration = duration,
                Service = service,
                Site = site,
                Status = status,
                AvailabilityStatus = availabilityStatus,
                CancellationReason = cancellationReason,
                CancellationNotificationStatus = cancellationNotificationStatus,
                Created = created,
                StatusUpdated = created,
                AttendeeDetails = new AttendeeDetails
                {
                    NhsNumber = nhsNumber,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    DateOfBirth = new DateOnly(2000, 1, 1)
                },
                ContactDetails =
                [
                    new ContactItem { Type = ContactItemType.Email, Value = GetContactInfo(ContactItemType.Email) },
                    new ContactItem { Type = ContactItemType.Phone, Value = GetContactInfo(ContactItemType.Phone) },
                    new ContactItem
                    {
                        Type = ContactItemType.Landline, Value = GetContactInfo(ContactItemType.Landline)
                    }
                ],
                AdditionalData = additionalData
            };

            var bookingIndex = new BookingIndexDocument
            {
                Reference = reference,
                Site = site,
                DocumentType = "booking_index",
                Id = reference,
                NhsNumber = nhsNumber,
                Status = status,
                Created = created,
                From = from
            };

            return (booking, bookingIndex);
        });
    }
    
    [And("the following bookings exist")]
    public async Task CreateBookings(DataTable dataTable)
    {
        foreach (var bookingAndIndex in BuildBookingAndIndexDocumentsFromDataTable(dataTable))
        {
            await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "booking_data"), bookingAndIndex.booking);
            await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "index_data"), bookingAndIndex.bookingIndex);
            MadeBookings.Add(bookingAndIndex.booking);
        }
    }

    [Then("the following bookings are now in the following state")]
    public async Task AssertBookings(DataTable dataTable)
    {
        var defaultReferenceOffset = 0;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var bookingReference = CreateCustomBookingReference(dataTable.GetRowValueOrDefault(row, "Reference")) ??
                                   BookingReferences.GetBookingReference(defaultReferenceOffset,
                                       BookingType.Confirmed);

            var siteId = GetSiteId(dataTable.GetRowValueOrDefault(row, "Site", DefaultSiteId));
            defaultReferenceOffset += 1;


            var booking = await Client.GetContainer("appts", "booking_data")
                .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));

            booking.Should().NotBeNull();

            var status = dataTable.GetEnumRowValueOrDefault<AppointmentStatus>(row, "Status");
            if (status != null)
            {
                booking.Resource.Status.Should().Be(status);
            }

            var cancellationReason = dataTable.GetEnumRowValueOrDefault<CancellationReason>(row, "Cancellation reason");
            if (cancellationReason != null)
            {
                booking.Resource.CancellationReason.Should().Be(cancellationReason);
            }

            var additionalData = BuildAdditionalDataFromDataTable(dataTable, row);
            if (additionalData != null)
            {
                booking.Resource.AdditionalData.Should().BeEquivalentTo(JObject.FromObject(additionalData));
            }
        }
    }
    protected async Task SetupBookings(string siteDesignation, DataTable dataTable, BookingType bookingType)
    {
        var bookings = dataTable.Rows.Skip(1).Select((row, index) =>
        {
            var customId = CreateCustomBookingReference(row.Cells.ElementAtOrDefault(4)?.Value);

            return new BookingDocument
            {
                Id = customId ??
                     BookingReferences.GetBookingReference(index, bookingType),
                DocumentType = "booking",
                Reference = customId ??
                            BookingReferences.GetBookingReference(index, bookingType),
                From =
                    DateTime.ParseExact(
                        $"{NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
                        "yyyy-MM-dd HH:mm", null),
                Duration = int.Parse(row.Cells.ElementAt(2).Value),
                Service = row.Cells.ElementAt(3).Value,
                Site = GetSiteId(siteDesignation),
                Status = MapStatus(bookingType),
                AvailabilityStatus = MapAvailabilityStatus(bookingType),
                Created = row.Cells.ElementAtOrDefault(5)?.Value is not null
                    ? DateTimeOffset.Parse(row.Cells.ElementAtOrDefault(5)?.Value)
                    : GetCreationDateTime(bookingType),
                StatusUpdated = GetCreationDateTime(bookingType),
                AttendeeDetails = new AttendeeDetails
                {
                    NhsNumber = NhsNumber,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    DateOfBirth = new DateOnly(2000, 1, 1)
                },
                ContactDetails =
                [
                    new ContactItem { Type = ContactItemType.Email, Value = GetContactInfo(ContactItemType.Email) },
                    new ContactItem { Type = ContactItemType.Phone, Value = GetContactInfo(ContactItemType.Phone) },
                    new ContactItem
                    {
                        Type = ContactItemType.Landline, Value = GetContactInfo(ContactItemType.Landline)
                    }
                ],
                AdditionalData = new { IsAppBooking = true }
            };
        });

        var bookingIndexDocuments = dataTable.Rows.Skip(1).Select((row, index) =>
        {
            var customId = CreateCustomBookingReference(row.Cells.ElementAtOrDefault(4)?.Value);

            return new BookingIndexDocument
            {
                Reference = customId ??
                            BookingReferences.GetBookingReference(index, bookingType),
                Site = GetSiteId(siteDesignation),
                DocumentType = "booking_index",
                Id = customId ??
                     BookingReferences.GetBookingReference(index, bookingType),
                NhsNumber = NhsNumber,
                Status = MapStatus(bookingType),
                Created = GetCreationDateTime(bookingType),
                From = DateTime.ParseExact(
                    $"{NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
                    "yyyy-MM-dd HH:mm", null),
            };
        });

        foreach (var booking in bookings)
        {
            await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "booking_data"), booking);
        }

        foreach (var bookingIndex in bookingIndexDocuments)
        {
            await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "index_data"), bookingIndex);
        }

        MadeBookings.AddRange(bookings);
    }

    protected string CreateCustomBookingReference(string identifier)
    {
        var customId = identifier;

        if (!string.IsNullOrWhiteSpace(customId))
        {
            customId += $"_{_testId}";
        }

        return customId;
    }

    [And(@"the original booking has been '(\w+)'")]
    public async Task AssertRescheduledBookingStatus(string outcome) => await AssertBookingStatus(outcome);

    [Then(@"the booking has been '(\w+)'")]
    public async Task AssertBookingStatus(string status)
    {
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertBookingStatusByReference(bookingReference, GetSiteId(), expectedStatus);
    }

    [Then(@"the booking at site '(.+)' has been '(\w+)'")]
    public async Task AssertBookingStatusAtSite(string siteId, string status)
    {
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertBookingStatusByReference(bookingReference, GetSiteId(siteId), expectedStatus);
    }

    [Then(@"the booking with reference '(.+)' has been '(.+)'")]
    [And(@"the booking with reference '(.+)' has been '(.+)'")]
    public async Task AssertSpecificBookingStatusChange(string bookingReference, string status)
    {
        var customId = CreateCustomBookingReference(bookingReference);
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);
        await AssertBookingStatusByReference(customId, GetSiteId(), expectedStatus);
    }

    [Then(@"the booking with reference '(.+)' has status '(.+)'")]
    [And(@"the booking with reference '(.+)' has status '(.+)'")]
    public async Task AssertSpecificBookingStatus(string bookingReference, string status)
    {
        var customId = CreateCustomBookingReference(bookingReference);
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);
        await AssertBookingStatusByReference(customId, GetSiteId(), expectedStatus, false);
    }

    [And("the booking should be deleted")]
    [Then("the booking should be deleted")]
    public async Task AssertBookingDeleted()
    {
        var siteId = GetSiteId();
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);

        var exception = await Assert.ThrowsAsync<CosmosException>(async () =>
            await Client.GetContainer("appts", "booking_data")
                .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId)));
        exception.Message.Should().Contain("404");

        exception = await Assert.ThrowsAsync<CosmosException>(async () =>
            await Client.GetContainer("appts", "index_data")
                .ReadItemAsync<BookingIndexDocument>(bookingReference, new PartitionKey("booking_index")));
        exception.Message.Should().Contain("404");
    }
    
    [And("the booking with reference '(.+)' should be deleted")]
    [Then("the booking with reference '(.+)' should be deleted")]
    public async Task AssertBookingDeleted(string bookingReference)
    {
        var siteId = GetSiteId();
        var customId = CreateCustomBookingReference(bookingReference);

        var exception = await Assert.ThrowsAsync<CosmosException>(async () =>
            await Client.GetContainer("appts", "booking_data")
                .ReadItemAsync<BookingDocument>(customId, new PartitionKey(siteId)));
        exception.Message.Should().Contain("404");

        exception = await Assert.ThrowsAsync<CosmosException>(async () =>
            await Client.GetContainer("appts", "index_data")
                .ReadItemAsync<BookingIndexDocument>(customId, new PartitionKey("booking_index")));
        exception.Message.Should().Contain("404");
    }

    [Then(@"the booking with reference '(.+)' has availability status '(.+)'")]
    [And(@"the booking with reference '(.+)' has availability status '(.+)'")]
    public async Task AssertSpecificAvailabilityStatus(string bookingReference, string status)
    {
        var customId = CreateCustomBookingReference(bookingReference);
        var expectedStatus = Enum.Parse<AvailabilityStatus>(status);
        await AssertAvailabilityStatusByReference(customId, expectedStatus, false);
    }

    [When(@"I create the following availability")]
    public async Task CreateAvailability(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.ToList();

            var date = cells.ElementAt(0).Value;
            var from = cells.ElementAt(1).Value;
            var until = cells.ElementAt(2).Value;
            var slotLength = cells.ElementAt(3).Value;
            var capacity = cells.ElementAt(4).Value;
            var services = cells.ElementAt(5).Value;

            var payload = new
            {
                date = NaturalLanguageDate.Parse(date).ToString("yyyy-MM-dd"),
                site = GetSiteId(),
                sessions = new[]
                {
                    new
                    {
                        from,
                        until,
                        slotLength = int.Parse(slotLength),
                        capacity = int.Parse(capacity),
                        services = services.Split(',').Select(s => s.Trim()).ToArray()
                    }
                },
                mode = "additive"
            };

            _response = await Http.PostAsJsonAsync("http://localhost:7071/api/availability", payload);
            _response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Then("I make the following bookings")]
    public async Task MakeBookings(DataTable dataTable)
    {
        var bookingIndex = 0;
        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.ToList();
            var date = cells.ElementAt(0).Value;
            var time = cells.ElementAt(1).Value;
            var duration = cells.ElementAt(2).Value;
            var service = cells.ElementAt(3).Value;

            object payload = new
            {
                from = DateTime.ParseExact(
                    $"{NaturalLanguageDate.Parse(date):yyyy-MM-dd} {time}",
                    "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm"),
                duration,
                service,
                site = GetSiteId(),
                kind = "booked",
                attendeeDetails = new
                {
                    nhsNumber = NhsNumber,
                    firstName = "John",
                    lastName = "Doe",
                    dateOfBirth = "1987-03-13"
                }
            };
            _response = await Http.PostAsJsonAsync("http://localhost:7071/api/booking", payload);
            _response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result =
                JsonConvert.DeserializeObject<MakeBookingResponse>(await _response.Content.ReadAsStringAsync());
            var bookingReference = result.BookingReference;

            _bookingReferences[bookingIndex] = bookingReference;
            bookingIndex += 1;
        }
    }

    [And("there are no sessions for '(.+)'")]
    public async Task AssertSessionNoLongerExists(string dateString)
    {
        var date = NaturalLanguageDate.Parse(dateString);
        var documentId = date.ToString("yyyyMMdd");

        var dayAvailabilityDocument = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<DailyAvailabilityDocument>(documentId, new PartitionKey(GetSiteId()));

        dayAvailabilityDocument.Resource.Sessions.Length.Should().Be(0);
    }

    [Given("the following sites exist in the system")]
    public async Task SetUpSites(DataTable dataTable)
    {
        var sites = dataTable.Rows.Skip(1).Select(
            row => new SiteDocument
            {
                Id = GetSiteId(dataTable.GetRowValueOrDefault(row, "Site")),
                Name = dataTable.GetRowValueOrDefault(row, "Name"),
                Address = dataTable.GetRowValueOrDefault(row, "Address"),
                PhoneNumber = dataTable.GetRowValueOrDefault(row, "PhoneNumber"),
                OdsCode = dataTable.GetRowValueOrDefault(row, "OdsCode"),
                Region = dataTable.GetRowValueOrDefault(row, "Region"),
                IntegratedCareBoard = dataTable.GetRowValueOrDefault(row, "ICB"),
                InformationForCitizens = dataTable.GetRowValueOrDefault(row, "InformationForCitizens"),
                DocumentType = "site",
                Accessibilities = ParseAccessibilities(dataTable.GetRowValueOrDefault(row, "Accessibilities")),
                Location = new Location("Point",
                    new[] { dataTable.GetDoubleRowValueOrDefault(row, "Longitude", -60d), dataTable.GetDoubleRowValueOrDefault(row, "Latitude", -60d) }),
                Type = dataTable.GetRowValueOrDefault(row, "Type"),
                IsDeleted = dataTable.GetBoolRowValueOrDefault(row, "IsDeleted")
            });

        foreach (var site in sites)
        {
            await CosmosAction_RetryOnTooManyRequests(CosmosAction.Create, Client.GetContainer("appts", "core_data"), site);
        }
    }

    protected static Accessibility[] ParseAccessibilities(string accessibilities)
    {
        if (string.IsNullOrWhiteSpace(accessibilities) || accessibilities == "__empty__")
        {
            return Array.Empty<Accessibility>();
        }

        var pairs = accessibilities.Split(",");
        return pairs.Select(p => p.Trim().Split("=")).Select(kvp => new Accessibility(kvp[0], kvp[1].ToLower())).ToArray();
    }

    private async Task AssertAvailabilityStatusByReference(string bookingReference, AvailabilityStatus status,
        bool expectStatusToHaveChanged = true)
    {
        var siteId = GetSiteId();
        var bookingDocument = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        bookingDocument.Resource.AvailabilityStatus.Should().Be(status);
        if (expectStatusToHaveChanged)
        {
            bookingDocument.Resource.StatusUpdated.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
        }
    }

    private async Task AssertBookingStatusByReference(string bookingReference, string siteId, AppointmentStatus status,
        bool expectStatusToHaveChanged = true)
    {
        var bookingDocument = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        bookingDocument.Resource.Status.Should().Be(status);
        if (expectStatusToHaveChanged)
        {
            bookingDocument.Resource.StatusUpdated.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
        }

        var indexDocument = await Client.GetContainer("appts", "index_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey("booking_index"));
        indexDocument.Resource.Status.Should().Be(status);
    }

    public async Task AssertCancellationReasonByReference(string bookingReference, CancellationReason cancellationReason)
    {
        var siteId = GetSiteId();
        var bookingDocument = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        bookingDocument.Resource.CancellationReason.Should().Be(cancellationReason);
    }

    protected static DateTime GetCreationDateTime(BookingType type) => type switch
    {
        BookingType.Recent => DateTime.UtcNow.AddHours(-18),
        BookingType.Confirmed => DateTime.UtcNow.AddHours(-48),
        BookingType.Provisional => DateTime.UtcNow.AddMinutes(-3),
        BookingType.ExpiredProvisional => DateTime.UtcNow.AddHours(-25),
        BookingType.Orphaned => DateTime.UtcNow.AddMinutes(-1),
        BookingType.Cancelled => DateTime.UtcNow.AddHours(-82),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    protected static DayOfWeek[] ParseDays(string pattern)
    {
        if (pattern == "All")
        {
            return new[]
            {
                DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                DayOfWeek.Friday, DayOfWeek.Saturday
            };
        }

        return pattern.Split(",").Select(d => Enum.Parse(typeof(DayOfWeek), d, true)).Cast<DayOfWeek>().ToArray();
    }

    private static string CreateRandomTenCharacterString()
    {
        var random = new Random();
        var randomString = string.Empty;
        for (var i = 0; i < 10; i++)
        {
            randomString = string.Concat(randomString, random.Next(10).ToString());
        }

        return randomString;
    }

    protected AppointmentStatus MapStatus(BookingType bookingType) => bookingType switch
    {
        BookingType.Recent => AppointmentStatus.Booked,
        BookingType.Confirmed => AppointmentStatus.Booked,
        BookingType.Provisional => AppointmentStatus.Provisional,
        BookingType.ExpiredProvisional => AppointmentStatus.Provisional,
        BookingType.Orphaned => AppointmentStatus.Booked,
        BookingType.Cancelled => AppointmentStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(bookingType)),
    };

    protected AvailabilityStatus MapAvailabilityStatus(BookingType bookingType) => bookingType switch
    {
        BookingType.Recent => AvailabilityStatus.Supported,
        BookingType.Confirmed => AvailabilityStatus.Supported,
        BookingType.Provisional => AvailabilityStatus.Supported,
        BookingType.ExpiredProvisional => AvailabilityStatus.Supported,
        BookingType.Orphaned => AvailabilityStatus.Orphaned,
        BookingType.Cancelled => AvailabilityStatus.Unknown,
        _ => throw new ArgumentOutOfRangeException(nameof(bookingType))
    };

    protected string GetSiteId(string siteDesignation = DefaultSiteId) =>
        $"{_testId}-{siteDesignation}";

    // TODO: Update to handle Okta on / off state
    protected string GetUserId(string userId) => $"{userId}_{_testId}@nhs.net";

    protected Dictionary<string, string> BuildAdditionalDataFromDataTable(DataTable table, TableRow row)
    {
        var keyValuePairs = new Dictionary<string, string>();
        for (var i = 1; i < 4; i++)
        {
            var columnName = $"AdditionalData {i}";
            var additionalData = table.GetRowValueOrDefault(row, columnName);
            if (additionalData != null)
            {
                var key = additionalData.Split(',')[0];
                var value = additionalData.Split(',')[1];
                keyValuePairs[key] = value;
            }
        }

        if (keyValuePairs.Count == 0)
        {
            return null;
        }

        return keyValuePairs;
    }

    protected static async Task<IEnumerable<TDocument>> RunQueryAsync<TDocument>(Container container,
        Expression<Func<TDocument, bool>> predicate)
    {
        var queryFeed = container.GetItemLinqQueryable<TDocument>().Where(predicate).ToFeedIterator();
        var results = new List<TDocument>();
        using (queryFeed)
        {
            while (queryFeed.HasMoreResults)
            {
                var resultSet = await queryFeed.ReadNextAsync();
                results.AddRange(resultSet);
            }
        }

        return results;
    }

    [GeneratedRegex(
        "^(?<format>Today|today|Tomorrow|tomorrow|Yesterday|yesterday|Next Monday|Next Tuesday|Next Wednesday|Next Thursday|Next Friday|Next Saturday|Next Sunday|(((?<magnitude>[0-9]+) (?<period>days|day|weeks|week|months|month|years|year) (?<direction>from|before) (now|today))))$")]
    private static partial Regex NaturalLanguageRelativeDate();
}

public class BookingReferenceManager
{
    private readonly Dictionary<int, string> _bookingReferences = new();

    public string GetBookingReference(int index, BaseFeatureSteps.BookingType bookingType,
        string customBookingReference = null)
    {
        if (bookingType != BaseFeatureSteps.BookingType.Confirmed && bookingType != BaseFeatureSteps.BookingType.Recent)
        {
            index += 50;
        }

        if (_bookingReferences.ContainsKey(index) == false)
        {
            _bookingReferences.Add(index, Guid.NewGuid().ToString());
        }

        return _bookingReferences[index];
    }
}

public enum CosmosAction
{
    Create = 0,
    Upsert = 1,
}
