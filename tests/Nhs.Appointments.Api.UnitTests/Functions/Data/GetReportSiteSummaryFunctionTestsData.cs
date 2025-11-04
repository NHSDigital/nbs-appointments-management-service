using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions.Data;

public static class GetReportSiteSummaryFunctionTestsData
{
    public static readonly User MockUser = new()
    {
        Id = "test.user2@testdomain.com",
        RoleAssignments =
        [
            new RoleAssignment
            {
                Role = "canned:availability-manager", Scope = "site:6877d86e-c2df-4def-8508-e1eccf0ea6ba"
            },
            new RoleAssignment
            {
                Role = "canned:availability-manager", Scope = "site:3ad2deb1-791b-452d-95dc-7090edd97f9a"
            }
        ]
    };

    public static readonly Site[] MockSites =
    [
        new(
            "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
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
            Type: string.Empty),
        new(
            "3ad2deb1-791b-452d-95dc-7090edd97f9a",
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
            Type: string.Empty)
    ];
}
