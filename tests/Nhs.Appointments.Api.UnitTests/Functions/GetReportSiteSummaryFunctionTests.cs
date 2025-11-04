using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Tests.Functions.Data;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetReportSiteSummaryFunctionTests
{
    private readonly GetReportSiteSummaryFunction _fut;

    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IPermissionChecker> _permissionChecker = new();
    private readonly Mock<ISiteReportService> _siteReportService = new();
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetAccessibilityDefinitionsFunction>> _mockLogger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    public GetReportSiteSummaryFunctionTests()
    {
        _featureToggleHelper
            .Setup(helper => helper.IsFeatureEnabled(Flags.SiteSummaryReport))
            .ReturnsAsync(true);

        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1)));

        _userService
            .Setup(service => service.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        _fut = new GetReportSiteSummaryFunction(
            _userService.Object,
            _permissionChecker.Object,
            _siteReportService.Object,
            _timeProvider.Object,
            _featureToggleHelper.Object,
            new SiteReportRequestValidator(),
            _userContextProvider.Object,
            _mockLogger.Object,
            _metricsRecorder.Object
        );
    }

    [Fact(DisplayName = "Returns NotImplemented when toggled OFF")]
    public async Task RunAsync_ReturnsNotImplemented_WhenToggledOff()
    {
        _featureToggleHelper
            .Setup(helper => helper.IsFeatureEnabled(Flags.SiteSummaryReport))
            .ReturnsAsync(false);

        var request = CreateRequest("2004-02-10", "2004-02-12");
        var result = await _fut.RunAsync(request);

        var contentResult = Assert.IsType<ContentResult>(result);
        contentResult.StatusCode.Should().Be(501);
        contentResult.Content.Should().Contain("Site Summary Reports are not enabled.");
    }

    [Fact(DisplayName = "Generates site summary reports")]
    public async Task RunAsync_Generates_Report()
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user2@testdomain.com");
        _userContextProvider.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);

        _userService.Setup(service => service.GetUserAsync("test.user2@testdomain.com"))
            .ReturnsAsync(GetReportSiteSummaryFunctionTestsData.MockUser);

        _permissionChecker.Setup(checker =>
                checker.GetSitesWithPermissionAsync("test.user2@testdomain.com", Permissions.ReportsSiteSummary))
            .ReturnsAsync(GetReportSiteSummaryFunctionTestsData.MockSites);

        _siteReportService.Setup(service => service.Generate(GetReportSiteSummaryFunctionTestsData.MockSites,
                new DateOnly(2004, 2, 10), new DateOnly(2004, 2, 12)))
            .ReturnsAsync(GetReportSiteSummaryFunctionTestsData.MockReports);

        var request = CreateRequest("2004-02-10", "2004-02-12");
        var result = await _fut.RunAsync(request);
        var fileContentResult = Assert.IsType<FileContentResult>(result);

        fileContentResult.ContentType.Should().Be("text/csv");
        fileContentResult.FileDownloadName.Should().Be("SiteReport_2004-02-10_2004-02-12_20200101010101.csv.csv");


        // Assert on csv headers
        var contentString = Encoding.UTF8.GetString(fileContentResult.FileContents);
        var csvLines = contentString.Split(Environment.NewLine);
        var headers = csvLines[0].Split(',');

        headers.Length.Should().Be(15);
        headers.Should().Contain("Site Name");
        headers.Should().Contain("ICB");
        headers.Should().Contain("ICB Name");
        headers.Should().Contain("Region");
        headers.Should().Contain("Region Name");
        headers.Should().Contain("ODS Code");
        headers.Should().Contain("Longitude");
        headers.Should().Contain("Latitude");
        headers.Should().Contain("RSV:Adult Booked");
        headers.Should().Contain("COVID:5_11 Booked");
        headers.Should().Contain("Total Bookings");
        headers.Should().Contain("Cancelled");
        headers.Should().Contain("Maximum Capacity");
        headers.Should().Contain("RSV:Adult Capacity");
        headers.Should().Contain("COVID:5_11 Capacity");


        csvLines.Length.Should().Be(5);

        // Report accuracy is tested in SiteReportServiceTests
        csvLines[1].Should().NotBeEmpty();
        csvLines[1].Split(',').Length.Should().Be(15);

        csvLines[2].Should().NotBeEmpty();
        csvLines[2].Split(',').Length.Should().Be(15);

        csvLines[3].Should().NotBeEmpty();
        csvLines[3].Split(',').Length.Should().Be(15);

        // TODO: Is this trailing empty line expected or a bug?
        csvLines[4].Should().Be(string.Empty);
    }

    private static HttpRequest CreateRequest(string startDate, string endDate)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(startDate))
        {
            queryParams.Add($"startDate={Uri.EscapeDataString(startDate)}");
        }

        if (!string.IsNullOrEmpty(endDate))
        {
            queryParams.Add($"endDate={Uri.EscapeDataString(endDate)}");
        }

        if (queryParams.Count > 0)
        {
            request.QueryString = new QueryString("?" + string.Join("&", queryParams));
        }

        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
