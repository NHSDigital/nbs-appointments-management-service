using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;
using Feature = Xunit.Gherkin.Quick.Feature;
using Location = Nhs.Appointments.Core.Location;
using Role = Nhs.Appointments.Persistance.Models.Role;
using RoleAssignment = Nhs.Appointments.Persistance.Models.RoleAssignment;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract partial class BaseFeatureSteps : Feature
{
    public enum BookingType { Recent, Confirmed, Provisional, ExpiredProvisional, Orphaned, Cancelled }

    protected const string ApiSigningKey =
        "2EitbEouxHQ0WerOy3TwcYxh3/wZA0LaGrU1xpKg0KJ352H/mK0fbPtXod0T0UCrgRHyVjF6JfQm/LillEZyEA==";

    private const string AppointmentsApiUrl = "http://localhost:7071/api";

    protected readonly Guid _testId = Guid.NewGuid();
    protected readonly CosmosClient Client;
    protected readonly HttpClient Http;
    protected readonly Mapper Mapper;
    protected List<BookingDocument> MadeBookings = new List<BookingDocument>();

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
        SetUpRoles().GetAwaiter().GetResult();
        SetUpIntegrationTestUserRoleAssignments().GetAwaiter().GetResult();
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
    public Task SetupSite()
    {
        var site = new SiteDocument
        {
            Id = GetSiteId(),
            DocumentType = "site",
            Location = new Location("point", new[] { 21.41416002128359, -157.77021027939483 })
        };
        return Client.GetContainer("appts", "core_data").CreateItemAsync(site);
    }

    // TODO: Added for BulkImport tests as it requires a valid Guid
    // To clean up this method & the one above so all siteId's use only a Guid
    [Given("a new site is configured for MYA")]
    public Task SetupNewSite()
    {
        var site = new SiteDocument
        {
            Id = _testId.ToString(),
            DocumentType = "site",
            OdsCode = "ODS1",
            IntegratedCareBoard = "ICB1",
            Region = "R1",
            Location = new Location("point", [1.41416002128359, 51.77021027939483])
        };
        return Client.GetContainer("appts", "core_data").CreateItemAsync(site);
    }

    protected async Task SetLocalFeatureToggleOverride(string name, string state)
    {
        var response = await Http.PatchAsync($"{AppointmentsApiUrl}/feature-flag-override/{name}?enabled={state}",
            null);

        response.EnsureSuccessStatusCode();
    }
    
    [Given("the following sessions exist for site '(.+)'")]
    [And("the following sessions exist for site '(.+)'")]
    public Task SetupSessionsForSite(string site, DataTable dataTable)
    {
        return SetupSessions(site, dataTable);
    }

    [Given("the following sessions")]
    [And("the following sessions")]
    public Task SetupSessions(DataTable dataTable)
    {
        return SetupSessions("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable);
    }

    public async Task SetupSessions(string siteDesignation, DataTable dataTable)
    {
        var site = GetSiteId(siteDesignation);
        var availabilityDocuments = DailyAvailabilityDocumentsFromTable(site, dataTable).ToList();
        foreach (var document in availabilityDocuments)
        {
            await Client.GetContainer("appts", "booking_data").CreateItemAsync(document);
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
            Id = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToString("yyyyMMdd"),
            Date = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value),
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

    public static DateOnly ParseNaturalLanguageDateOnly(string dateString)
    {
        var match = NaturalLanguageRelativeDate().Match(dateString);
        if (!match.Success)
        {
            throw new FormatException("Date string not recognised.");
        }

        var format = match.Groups["format"].Value;
        switch (format)
        {
            case "Tomorrow":
            case "tomorrow":
                return DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            case "Yesterday":
            case "yesterday":
                return DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            case "Today":
            case "today":
                return DateOnly.FromDateTime(DateTime.UtcNow);
            case "Next Monday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Monday));
            case "Next Tuesday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Tuesday));
            case "Next Wednesday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Wednesday));
            case "Next Thursday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Thursday));
            case "Next Friday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Friday));
            case "Next Saturday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Saturday));
            case "Next Sunday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Sunday));
        }

        var period = match.Groups["period"].Value;
        var direction = match.Groups["direction"].Value;
        var magnitude = match.Groups["magnitude"].Value;

        var offset = direction == "from" ? int.Parse(magnitude) : int.Parse(magnitude) * -1;
        return period switch
        {
            "days" => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset),
            "day" => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset),
            "weeks" => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset * 7),
            "week" => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset * 7),
            "months" => DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(offset),
            "month" => DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(offset),
            "years" => DateOnly.FromDateTime(DateTime.UtcNow).AddYears(offset),
            "year" => DateOnly.FromDateTime(DateTime.UtcNow).AddYears(offset),
            _ => throw new FormatException("Error parsing natural language date regex")
        };
    }

    /// <summary>
    /// Want to return a day of the week in the next week.
    /// </summary>
    /// <param name="targetDay"></param>
    /// <returns></returns>
    private static DateTime GetDayInNextWeek(DayOfWeek targetDay)
    {
        var today = DateTime.UtcNow;

        // Get this week's Monday
        var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var thisWeeksMonday = today.AddDays(-daysSinceMonday);

        // Get next week's Monday
        var nextWeeksMonday = thisWeeksMonday.AddDays(7);

        // Calculate days to target day from next Monday
        var daysToTarget = ((int)targetDay - (int)DayOfWeek.Monday + 7) % 7;

        return nextWeeksMonday.AddDays(daysToTarget);
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
    public Task SetupBookings(DataTable dataTable)
    {
        return SetupBookings("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable, BookingType.Confirmed);
    }
    
    [Given("the following bookings have been made for site '(.+)'")]
    [And("the following bookings have been made for site '(.+)'")]
    public Task SetupBookingsForSite(string site, DataTable dataTable)
    {
        return SetupBookings(site, dataTable, BookingType.Confirmed);
    }

    [Given("the following recent bookings have been made")]
    [And("the following recent bookings have been made")]
    public Task SetupRecentBookings(DataTable dataTable)
    {
        return SetupBookings("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable, BookingType.Recent);
    }

    [Given("the following provisional bookings have been made")]
    [And("the following provisional bookings have been made")]
    public Task SetupProvisionalBookings(DataTable dataTable)
    {
        return SetupBookings("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable, BookingType.Provisional);
    }

    [Given("the following expired provisional bookings have been made")]
    [And("the following expired provisional bookings have been made")]
    public Task SetupExpiredProvisionalBookings(DataTable dataTable)
    {
        return SetupBookings("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable, BookingType.ExpiredProvisional);
    }

    [Given("the following cancelled bookings have been made")]
    [And("the following cancelled bookings have been made")]
    public Task SetupCancelledBookings(DataTable dataTable) =>
        SetupBookings("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable, BookingType.Cancelled);

    [And("the following orphaned bookings exist")]
    public Task SetupOrphanedBookings(DataTable dataTable) =>
        SetupBookings("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable, BookingType.Orphaned);
    
    [And("the following orphaned bookings exist for site '(.+)'")]
    public Task SetupOrphanedBookingsForSite(string site, DataTable dataTable) =>
        SetupBookings(site, dataTable, BookingType.Orphaned);

    protected DateTime ParseDayAndTime(string day, string time) => DateTime.ParseExact(
        $"{ParseNaturalLanguageDateOnly(day).ToString("yyyy-MM-dd")} {time}",
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
            var site = GetSiteId(dataTable.GetRowValueOrDefault(row, "Site", "beeae4e0-dd4a-4e3a-8f4d-738f9418fb51"));
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
            await Client.GetContainer("appts", "booking_data").CreateItemAsync(bookingAndIndex.booking);
            await Client.GetContainer("appts", "index_data").CreateItemAsync(bookingAndIndex.bookingIndex);

            MadeBookings.Add(bookingAndIndex.booking);
        }
    }

    [Then("the following bookings are now in the following state")]
    public async Task AssertBookings(DataTable dataTable)
    {
        var siteId = GetSiteId();
        var defaultReferenceOffset = 0;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var bookingReference = CreateCustomBookingReference(dataTable.GetRowValueOrDefault(row, "Reference")) ??
                                   BookingReferences.GetBookingReference(defaultReferenceOffset,
                                       BookingType.Confirmed);
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
                booking.Resource.AdditionalData.Should().Be(additionalData);
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
                        $"{ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
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
                    $"{ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
                    "yyyy-MM-dd HH:mm", null),
            };
        });

        foreach (var booking in bookings)
        {
            await Client.GetContainer("appts", "booking_data").CreateItemAsync(booking);
        }

        foreach (var bookingIndex in bookingIndexDocuments)
        {
            await Client.GetContainer("appts", "index_data").CreateItemAsync(bookingIndex);
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
    public Task AssertRescheduledBookingStatus(string outcome) => AssertBookingStatus(outcome);

    [Then(@"the booking has been '(\w+)'")]
    public async Task AssertBookingStatus(string status)
    {
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertBookingStatusByReference(bookingReference, expectedStatus);
    }

    [Then(@"the booking with reference '(.+)' has been '(.+)'")]
    [And(@"the booking with reference '(.+)' has been '(.+)'")]
    public async Task AssertSpecificBookingStatusChange(string bookingReference, string status)
    {
        var customId = CreateCustomBookingReference(bookingReference);
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);
        await AssertBookingStatusByReference(customId, expectedStatus);
    }

    [Then(@"the booking with reference '(.+)' has status '(.+)'")]
    [And(@"the booking with reference '(.+)' has status '(.+)'")]
    public async Task AssertSpecificBookingStatus(string bookingReference, string status)
    {
        var customId = CreateCustomBookingReference(bookingReference);
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);
        await AssertBookingStatusByReference(customId, expectedStatus, false);
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

    private async Task AssertBookingStatusByReference(string bookingReference, AppointmentStatus status,
        bool expectStatusToHaveChanged = true)
    {
        var siteId = GetSiteId();
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

    protected string GetSiteId(string siteDesignation = "beeae4e0-dd4a-4e3a-8f4d-738f9418fb51") =>
        $"{_testId}-{siteDesignation}";

    // TODO: Update to handle Okta on / off state
    protected string GetUserId(string userId) => $"{userId}_{_testId}@nhs.net";

    private async Task SetUpRoles()
    {
        var roles = new RolesDocument
        {
            Id = "global_roles",
            DocumentType = "system",
            Roles =
            [
                new Role
                {
                    Id = "system:integration-test-user",
                    Name = "Integration Test Api User Role",
                    Description = "Role for integration test user.",
                    Permissions =
                    [
                        Permissions.MakeBooking,
                        Permissions.QueryBooking,
                        Permissions.CancelBooking,
                        Permissions.ManageUsers,
                        Permissions.ViewUsers,
                        Permissions.QuerySites,
                        Permissions.ViewSiteMetadata,
                        Permissions.ViewSite,
                        Permissions.ViewSitePreview,
                        Permissions.ManageSite,
                        Permissions.ManageSiteAdmin,
                        Permissions.QueryAvailability,
                        Permissions.SetupAvailability,
                        Permissions.SystemRunProvisionalSweeper,
                        Permissions.SystemRunReminders,
                        Permissions.SystemDataImporter
                    ]
                },
                new Role
                {
                    Id = "canned:availability-manager",
                    Name = "Availability manager",
                    Description = "A user can create, view, and manage site availability.",
                    Permissions =
                    [
                        Permissions.SetupAvailability, Permissions.QueryAvailability, Permissions.QueryBooking,
                        Permissions.ViewSite, Permissions.ViewSitePreview, Permissions.ViewSiteMetadata
                    ]
                },
                new Role
                {
                    Id = "canned:appointment-manager",
                    Name = "Appointment manager",
                    Description = "A user can view and cancel appointments.",
                    Permissions =
                    [
                        Permissions.QueryAvailability, Permissions.CancelBooking, Permissions.QueryBooking,
                        Permissions.ViewSite, Permissions.ViewSitePreview, Permissions.ViewSiteMetadata
                    ]
                },
                new Role
                {
                    Id = "canned:site-details-manager",
                    Name = "Site details manager",
                    Description = "A user can edit site details and accessibility information.",
                    Permissions =
                    [
                        Permissions.QueryAvailability, Permissions.QueryBooking, Permissions.ViewSite,
                        Permissions.ViewSitePreview, Permissions.ManageSite, Permissions.ViewSiteMetadata
                    ]
                },
                new Role
                {
                    Id = "canned:user-manager",
                    Name = "User manager",
                    Description = "A user can view and manage user role assignments.",
                    Permissions =
                    [
                        Permissions.QueryAvailability, Permissions.QueryBooking, Permissions.ViewSite,
                        Permissions.ViewSitePreview, Permissions.ManageSite, Permissions.ViewSiteMetadata,
                        Permissions.ViewUsers, Permissions.ManageUsers
                    ]
                },
            ]
        };
        await Client.GetContainer("appts", "core_data").UpsertItemAsync(roles);
    }

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

    private async Task SetUpNotificationConfiguration()
    {
        var notificationConfiguration = new NotificationConfigurationDocument
        {
            Id = "notification_configuration",
            DocumentType = "system",
            Configs =
            [
                new NotificationConfigurationItem
                {
                    Services = ["COVID", "COVID:18_74"],
                    EmailTemplateId = "COVID Email Confirmation",
                    SmsTemplateId = "COVID SMS Confirmation",
                    EventType = "BookingMade"
                },
                new NotificationConfigurationItem
                {
                    Services = ["COVID", "COVID:18_74"],
                    EmailTemplateId = "COVID Email Reminder",
                    SmsTemplateId = "COVID SMS Reminder",
                    EventType = "BookingReminder"
                }
            ]
        };
        await Client.GetContainer("appts", "core_data").UpsertItemAsync(notificationConfiguration);
    }

    protected async Task SetUpIntegrationTestUserRoleAssignments(DateOnly latestAcceptedEulaVersion = default)
    {
        var userAssignments = new UserDocument
        {
            Id = "api@test",
            ApiSigningKey = ApiSigningKey,
            DocumentType = "user",
            RoleAssignments =
            [
                new RoleAssignment { Role = "system:integration-test-user", Scope = "global" }
            ],
            LatestAcceptedEulaVersion = latestAcceptedEulaVersion
        };
        await Client.GetContainer("appts", "core_data").UpsertItemAsync(userAssignments);
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
