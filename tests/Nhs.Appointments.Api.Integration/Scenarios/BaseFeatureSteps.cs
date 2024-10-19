using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.ApiClient.Auth;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using Role = Nhs.Appointments.Persistance.Models.Role;
using RoleAssignment = Nhs.Appointments.Persistance.Models.RoleAssignment;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract class BaseFeatureSteps : Feature
{
    private const string ApiSigningKey = "2EitbEouxHQ0WerOy3TwcYxh3/wZA0LaGrU1xpKg0KJ352H/mK0fbPtXod0T0UCrgRHyVjF6JfQm/LillEZyEA==";
    protected const string AppointmentsApiUrl = "http://localhost:7071/api";
    protected readonly CosmosClient Client;
    protected readonly HttpClient Http;
    protected readonly Mapper Mapper;
    private readonly Guid _testId = Guid.NewGuid();
    private readonly string _nhsNumber = CreateRandomTenCharacterString();
        
    protected string NhsNumber => _nhsNumber;
    private string BookingReference => ReverseString(NhsNumber);

    public BaseFeatureSteps()
    {
        CosmosClientOptions options = new()
        {
            HttpClientFactory = () => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
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
            authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_TOKEN") ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            clientOptions: options);

        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CosmosAutoMapperProfile>();
        });
        Mapper = new Mapper(mapperConfiguration);
        SetUpRoles().GetAwaiter().GetResult();
        SetUpIntegrationTestUserRoleAssignments().GetAwaiter().GetResult();
    }

    [Given("the site is configured for MYA")]
    public Task SetupSite()
    {
        var site = new SiteDocument
        {
            Id = GetSiteId(),
            DocumentType = "site",
            Location = new Location("point", new[] { 21.41416002128359, -157.77021027939483 } )
        };
        return Client.GetContainer("appts", "index_data").CreateItemAsync(site);
    }

    [Given("the following sessions")]
    [And("the following sessions")]
    public Task SetupSessions(Gherkin.Ast.DataTable dataTable)
    {
        return SetupSessions("A", dataTable);
    }

    [Given(@"the following sessions for site '(\w)'")]
    [And(@"the following sessions for site '(\w)'")]
    public async Task SetupSessions(string siteDesignation, Gherkin.Ast.DataTable dataTable)
    {
        var sessions = dataTable.Rows.Skip(1).Select((row, index) => new DailyAvailabilityDocument
        {
            Id = row.Cells.ElementAt(0).Value.Replace("-", ""),
            Date = DateOnly.ParseExact(row.Cells.ElementAt(0).Value, "yyyy-MM-dd"),
            Site = GetSiteId(siteDesignation),
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

        var documents = sessions.GroupBy(s => s.Date).Select(g => new DailyAvailabilityDocument()
        {
            Id = g.First().Id,
            Date = g.Key,
            Site = g.First().Site,
            DocumentType = "daily_availability",
            Sessions = g.SelectMany(s => s.Sessions).ToArray()
        });
        
        foreach (var document in documents)
        {
            await Client.GetContainer("appts", "booking_data").CreateItemAsync(document);
        }
    }

    [Given("the following bookings have been made")]
    [And("the following bookings have been made")]
    public Task SetupBookings(Gherkin.Ast.DataTable dataTable)
    {
        return SetupBookings("A", dataTable);
    }

    [And(@"the following bookings have been made for site '(\w)'")]
    public async Task SetupBookings(string siteDesignation, Gherkin.Ast.DataTable dataTable)
    {
        var bookings = dataTable.Rows.Skip(1).Select((row, index) => new BookingDocument
        {
            Id = GetBookingReference(index.ToString()),
            DocumentType = "booking",
            Reference = GetBookingReference(index.ToString()),
            From = DateTime.ParseExact($"{row.Cells.ElementAt(0).Value} {row.Cells.ElementAt(1).Value}", "yyyy-MM-dd HH:mm", null),
            Duration = int.Parse(row.Cells.ElementAt(2).Value),
            Service = row.Cells.ElementAt(3).Value,
            Site = GetSiteId(siteDesignation),
            AttendeeDetails = new Core.AttendeeDetails
            {
                NhsNumber = NhsNumber,
                FirstName = "FirstName",
                LastName = "LastName",
                DateOfBirth = new DateOnly(2000, 1, 1)
            }
        });

        var bookingIndexDocuments = dataTable.Rows.Skip(1).Select(
            (row, index) => new BookingIndexDocument
            {
                Reference = GetBookingReference(index.ToString()),
                Site = GetSiteId(),
                DocumentType = "booking_index",
                Id = GetBookingReference(index.ToString()),
                NhsNumber = NhsNumber
            });

        foreach (var booking in bookings)
        {            
            await Client.GetContainer("appts", "booking_data").CreateItemAsync(booking);
        }
        
        foreach (var bookingIndex in bookingIndexDocuments)
        {            
            await Client.GetContainer("appts", "index_data").CreateItemAsync(bookingIndex);
        }
    }

    private DayOfWeek[] ParseDays(string pattern)
    {
        if (pattern == "All")
            return new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        else
            return pattern.Split(",").Select(d => Enum.Parse(typeof(DayOfWeek), d, true)).Cast<DayOfWeek>().ToArray();
    }
    
    private static string CreateRandomTenCharacterString()
    {
        var random = new Random();
        var randomString = string.Empty;
        for (int i = 0; i < 10; i++)
            randomString = string.Concat(randomString, random.Next(10).ToString());
        return randomString;
    }

    private static string ReverseString(string stringToReverse) => new (stringToReverse.Reverse().ToArray());
    protected string GetTestId => $"{_testId}";
    protected string GetSiteId(string siteDesignation = "A") => $"{_testId}-{siteDesignation}";
    protected string GetUserId(string userId) => $"{userId}@{_testId}.com";
    protected string GetBookingReference(string index = "0") => $"{BookingReference}-{index}";

    private async Task SetUpRoles()
    {
        var roles = new RolesDocument()
        {
            Id = "global_roles",
            DocumentType = "roles",
            Roles = [
                new Role
                    { 
                        Id = "system:integration-test-user", 
                        Name = "Integration Test Api User Role",
                        Description = "Role for integration test user.",
                        Permissions = [
                            "site:get-meta-data", 
                            "availability:query", 
                            "booking:make", 
                            "booking:query", 
                            "booking:cancel", 
                            "site:get-config", 
                            "site:set-config", 
                            "availability:get-setup", 
                            "users:manage", 
                            "users:view", 
                            "sites:query",
                            "site:view",
                            "site:manage"
                        ] 
                    },
                new Role
                { 
                    Id = "canned:site-configuration-manager", 
                    Name = "Site configuration manager",
                    Description = "A user can view and manage site information, such as access needs.",
                    Permissions = ["site:get-config", "site:set-config" ] 
                },
                new Role
                { 
                    Id = "canned:availability-manager", 
                    Name = "Availability manager",
                    Description = "A user can create, view, and manage site availability.",
                    Permissions = ["availability:get-setup", "availability:set-setup", "availability:query"] 
                },
                new Role
                { 
                    Id = "canned:appointment-manager", 
                    Name = "Appointment manager",
                    Description = "A user can create, edit, and cancel bookings.",
                    Permissions = ["booking:make", "booking:query", "booking:cancel"] 
                },
                new Role
                { 
                    Id = "canned:check-in", 
                    Name = "Check-in",
                    Description = "A user can check in/undo check in patients for their bookings.",
                    Permissions = ["booking:query", "booking:set-status"] 
                }
            ]
        };        
        await Client.GetContainer("appts", "index_data").UpsertItemAsync(roles);
    }
    
    private async Task SetUpIntegrationTestUserRoleAssignments()
    {
        var userAssignments = new UserDocument()
        {
            
            Id = "api@test",
            ApiSigningKey = ApiSigningKey,
            DocumentType = "user",
            RoleAssignments = [
                new RoleAssignment()
                    { Role = "system:integration-test-user", Scope = "global" }
            ]
        };        
        await Client.GetContainer("appts", "index_data").UpsertItemAsync(userAssignments);
    }
}