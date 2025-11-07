using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Core.UnitTests.Reports;

public static class SiteReportServiceMockData
{
    public static readonly Guid Site1Guid = new("6877d86e-c2df-4def-8508-e1eccf0ea6ba");
    public static readonly Guid Site2Guid = new("3ad2deb1-791b-452d-95dc-7090edd97f9a");
    public static readonly Guid Site3Guid = new("0c06c137-2f8a-4334-a594-0632d0407966");

    public static readonly DateOnly DayOne = new(2004, 2, 10);
    public static readonly DateOnly DayTwo = new(2004, 2, 11);
    public static readonly DateOnly DayThree = new(2004, 2, 12);

    public static readonly Site[] MockSites =
    [
        new(
            Site1Guid.ToString(),
            "Site 1",
            "1 Park Row",
            "0113 1111111",
            "ABC01",
            "R1",
            "ICB1",
            Location: new Location("Point", [.505, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Online, isDeleted: null,
            Type: "GP Practice"),
        new(
            Site2Guid.ToString(),
            "Site 2",
            "2 Park Row",
            "0113 1111111",
            "ABC02",
            "R1",
            "ICB1",
            Location: new Location("Point", [.506, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Online, isDeleted: null,
            Type: "GP Practice"),
        new(
            Site3Guid.ToString(),
            "Site 3",
            "3 Park Row",
            "0113 1111111",
            "ABC03",
            "R1",
            "ICB1",
            Location: new Location("Point", [.507, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Online, isDeleted: null,
            Type: "GP Practice")
    ];

    public static readonly ClinicalServiceType[] MockClinicalServices =
    [
        new() { Value = "RSV:Adult", Label = "RSV Adult", ServiceType = "RSV", Url = "https://www.nhs.uk/book-rsv" },
        new()
        {
            Value = "COVID:5_11",
            Label = "COVID 5-11",
            ServiceType = "COVID-19",
            Url = "https://www.nhs.uk/bookcovid"
        }
    ];

    public static readonly string[] MockClinicalServiceIds = MockClinicalServices.Select(x => x.Value)
        .ToArray();

    public static readonly WellKnownOdsEntry[] MockWellKnownOdsCodes =
    [
        new("R1", "Region One", "region"), new("R2", "Region Two", "region"),
        new("ICB1", "Integrated Care Board One", "icb"),
        new("ICB2", "Integrated Care Board Two", "icb")
    ];

    public static readonly DailySiteSummary[] MockSite1DailySummaries =
    [
        new()
        {
            Site = Site1Guid.ToString(),
            Date = DayOne,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 35 }, { "COVID:5_11", 78 } },
            Cancelled = 29,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 7 }, { "COVID:5_11", 23 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 65 }, { "COVID:5_11", 22 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        },
        new()
        {
            Site = Site1Guid.ToString(),
            Date = DayTwo,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 38 }, { "COVID:5_11", 67 } },
            Cancelled = 22,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 8 }, { "COVID:5_11", 15 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 62 }, { "COVID:5_11", 33 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        },
        new()
        {
            Site = Site1Guid.ToString(),
            Date = DayThree,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 29 }, { "COVID:5_11", 96 } },
            Cancelled = 35,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 2 }, { "COVID:5_11", 22 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 71 }, { "COVID:5_11", 4 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        }
    ];

    public static readonly DailySiteSummary[] MockSite2DailySummaries =
    [
        new()
        {
            Site = Site2Guid.ToString(),
            Date = DayOne,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 42 }, { "COVID:5_11", 58 } },
            Cancelled = 15,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 5 }, { "COVID:5_11", 12 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 58 }, { "COVID:5_11", 42 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        },
        new()
        {
            Site = Site2Guid.ToString(),
            Date = DayTwo,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 45 }, { "COVID:5_11", 85 } },
            Cancelled = 18,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 6 }, { "COVID:5_11", 20 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 55 }, { "COVID:5_11", 15 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        },
        new()
        {
            Site = Site2Guid.ToString(),
            Date = DayThree,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 52 }, { "COVID:5_11", 73 } },
            Cancelled = 25,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 9 }, { "COVID:5_11", 17 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 48 }, { "COVID:5_11", 27 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        }
    ];

    public static readonly DailySiteSummary[] MockSite3DailySummaries =
    [
        new()
        {
            Site = Site3Guid.ToString(),
            Date = DayOne,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 28 }, { "COVID:5_11", 92 } },
            Cancelled = 33,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 3 }, { "COVID:5_11", 18 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 72 }, { "COVID:5_11", 8 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        },

        new()
        {
            Site = Site3Guid.ToString(),
            Date = DayTwo,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 31 }, { "COVID:5_11", 74 } },
            Cancelled = 27,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 4 }, { "COVID:5_11", 16 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 69 }, { "COVID:5_11", 26 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        },
        new()
        {
            Site = Site3Guid.ToString(),
            Date = DayThree,
            Bookings = new Dictionary<string, int> { { "RSV:Adult", 47 }, { "COVID:5_11", 68 } },
            Cancelled = 31,
            Orphaned = new Dictionary<string, int> { { "RSV:Adult", 6 }, { "COVID:5_11", 14 } },
            RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", 53 }, { "COVID:5_11", 32 } },
            MaximumCapacity = 200,
            GeneratedAtUtc = new DateTime(2020, 1, 1, 1, 1, 1)
        }
    ];
}
