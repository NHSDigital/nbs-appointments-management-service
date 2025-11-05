using Nhs.Appointments.Core;
using Nhs.Appointments.Core.UnitTests.Reports;

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

    public static readonly Site[] MockSites = SiteReportServiceMockData.MockSites;

    public static readonly SiteReport[] MockReports =
    [
        new(
            SiteReportServiceMockData.MockSites[0],
            SiteReportServiceMockData.MockSite1DailySummaries,
            SiteReportServiceMockData.MockClinicalServiceIds,
            SiteReportServiceMockData.MockWellKnownOdsCodes),
        new(
            SiteReportServiceMockData.MockSites[1],
            SiteReportServiceMockData.MockSite2DailySummaries,
            SiteReportServiceMockData.MockClinicalServiceIds,
            SiteReportServiceMockData.MockWellKnownOdsCodes),
        new(
            SiteReportServiceMockData.MockSites[2],
            SiteReportServiceMockData.MockSite3DailySummaries,
            SiteReportServiceMockData.MockClinicalServiceIds,
            SiteReportServiceMockData.MockWellKnownOdsCodes)
    ];
}
