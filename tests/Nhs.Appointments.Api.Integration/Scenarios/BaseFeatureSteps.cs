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
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.ApiClient.Auth;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;
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

    protected const string AppointmentsApiUrl = "http://localhost:7071/api";
    private readonly Guid _testId = Guid.NewGuid();
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
        SetUpNotificationConfiguration().GetAwaiter().GetResult();
    }

    protected string NhsNumber { get; private set; } = CreateRandomTenCharacterString();

    protected string GetTestId => $"{_testId}";

    public BookingReferenceManager BookingReferences { get; } = new();

    protected string GetContactInfo(ContactItemType type) => type switch
    {
        ContactItemType.Landline => $"0113{NhsNumber.Substring(7)}",
        ContactItemType.Email => $"{NhsNumber}@test.com",
        ContactItemType.Phone => $"07777{NhsNumber.Substring(6)}",
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

    [Given("the following sessions")]
    [And("the following sessions")]
    public Task SetupSessions(DataTable dataTable)
    {
        return SetupSessions("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable);
    }

    [Given(@"the following sessions for site '(\w)'")]
    [And(@"the following sessions for site '(\w)'")]
    public async Task SetupSessions(string siteDesignation, DataTable dataTable)
    {
        var site = GetSiteId(siteDesignation);
        var availabilityDocuments = DailyAvailabilityDocumentsFromTable(site, dataTable);
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
        return SetupBookings("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable);
    }

    [And(@"the following bookings have been made for site '(\w)'")]
    public Task SetupBookings(string siteDesignation, DataTable dataTable)
    {
        return SetupBookings(siteDesignation, dataTable, BookingType.Confirmed);
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
    public Task SetupOrphanedBookings(DataTable dataTable) => SetupBookings("beeae4e0-dd4a-4e3a-8f4d-738f9418fb51", dataTable, BookingType.Orphaned);

    protected async Task SetupBookings(string siteDesignation, DataTable dataTable, BookingType bookingType)
    {
        var bookings = dataTable.Rows.Skip(1).Select((row, index) => new BookingDocument
        {
            Id = row.Cells.ElementAtOrDefault(4)?.Value ??
                 BookingReferences.GetBookingReference(index, bookingType),
            DocumentType = "booking",
            Reference = row.Cells.ElementAtOrDefault(4)?.Value ??
                        BookingReferences.GetBookingReference(index, bookingType),
            From =
                DateTime.ParseExact(
                    $"{ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
                    "yyyy-MM-dd HH:mm", null),
            Duration = int.Parse(row.Cells.ElementAt(2).Value),
            Service = row.Cells.ElementAt(3).Value,
            Site = GetSiteId(siteDesignation),
            Status = MapStatus(bookingType),
            Created = row.Cells.ElementAtOrDefault(5)?.Value is not null
                ? DateTimeOffset.Parse(row.Cells.ElementAtOrDefault(5)?.Value)
                : GetCreationDateTime(bookingType),
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
                new ContactItem { Type = ContactItemType.Landline, Value = GetContactInfo(ContactItemType.Landline) }
            ],
            AdditionalData = new { IsAppBooking = true }
        });

        var bookingIndexDocuments = dataTable.Rows.Skip(1).Select(
            (row, index) => new BookingIndexDocument
            {
                Reference = row.Cells.ElementAtOrDefault(4)?.Value ??
                            BookingReferences.GetBookingReference(index, bookingType),
                Site = GetSiteId(),
                DocumentType = "booking_index",
                Id = row.Cells.ElementAtOrDefault(4)?.Value ??
                     BookingReferences.GetBookingReference(index, bookingType),
                NhsNumber = NhsNumber,
                Status = MapStatus(bookingType),
                Created = GetCreationDateTime(bookingType),
                From = DateTime.ParseExact(
                    $"{ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
                    "yyyy-MM-dd HH:mm", null),
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
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);

        await AssertBookingStatusByReference(bookingReference, expectedStatus);
    }

    [Then(@"the booking with reference '(.+)' has status '(.+)'")]
    [And(@"the booking with reference '(.+)' has status '(.+)'")]
    public async Task AssertSpecificBookingStatus(string bookingReference, string status)
    {
        var expectedStatus = Enum.Parse<AppointmentStatus>(status);

        await AssertBookingStatusByReference(bookingReference, expectedStatus, false);
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

    protected static DateTime GetCreationDateTime(BookingType type) => type switch
    {
        BookingType.Recent => DateTime.UtcNow.AddHours(-18),
        BookingType.Confirmed => DateTime.UtcNow.AddHours(-48),
        BookingType.Provisional => DateTime.UtcNow.AddMinutes(-2),
        BookingType.ExpiredProvisional => DateTime.UtcNow.AddMinutes(-8),
        BookingType.Orphaned => DateTime.UtcNow.AddHours(-64),
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
        BookingType.Orphaned => AppointmentStatus.Orphaned,
        BookingType.Cancelled => AppointmentStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(bookingType)),
    };

    protected string GetSiteId(string siteDesignation = "beeae4e0-dd4a-4e3a-8f4d-738f9418fb51") =>
        $"{_testId}-{siteDesignation}";

    protected string GetUserId(string userId) => $"{userId}@{_testId}.com";

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
                        "site:get-meta-data",
                        "availability:query",
                        "booking:make",
                        "booking:query",
                        "booking:cancel",
                        "users:manage",
                        "users:view",
                        "sites:query",
                        "site:view",
                        "site:manage",
                        "availability:setup",
                        "system:run-provisional-sweep",
                        "system:run-reminders"
                    ]
                },
                new Role
                {
                    Id = "canned:availability-manager",
                    Name = "Availability manager",
                    Description = "A user can create, view, and manage site availability.",
                    Permissions =
                    [
                        "availability:setup", "availability:query", "booking:query", "site:view",
                        "site:get-meta-data"
                    ]
                },
                new Role
                {
                    Id = "canned:appointment-manager",
                    Name = "Appointment manager",
                    Description = "A user can cancel appointments.",
                    Permissions =
                        ["availability:query", "booking:cancel", "booking:query", "site:view", "site:get-meta-data"]
                },
                new Role
                {
                    Id = "canned:site-details-manager",
                    Name = "Site details manager",
                    Description = "A user can edit site details and accessibility information.",
                    Permissions =
                        ["availability:query", "booking:query", "site:view", "site:manage", "site:get-meta-data"]
                },
            ]
        };
        await Client.GetContainer("appts", "core_data").UpsertItemAsync(roles);
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
        "^(?<format>Today|today|Tomorrow|tomorrow|Yesterday|yesterday|(((?<magnitude>[0-9]+) (?<period>days|day|weeks|week|months|month|years|year) (?<direction>from|before) (now|today))))$")]
    private static partial Regex NaturalLanguageRelativeDate();
}

public class BookingReferenceManager
{
    private readonly Dictionary<int, string> _bookingReferences = new();

    public string GetBookingReference(int index, BaseFeatureSteps.BookingType bookingType)
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
