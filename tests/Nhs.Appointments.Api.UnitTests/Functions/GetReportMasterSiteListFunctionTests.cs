using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Reports.MasterSiteList;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetReportMasterSiteListFunctionTests
{
    private readonly GetReportMasterSiteListFunction _sut;

    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetReportMasterSiteListFunction>> _mockLogger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<TimeProvider> _timeProvider = new();

    public GetReportMasterSiteListFunctionTests()
    {
        _featureToggleHelper
            .Setup(helper => helper.IsFeatureEnabled(Flags.ReportsUplift))
            .ReturnsAsync(true);

        _sut = new GetReportMasterSiteListFunction(
            _siteService.Object,
            new MasterSiteListReportCsvWriter(_timeProvider.Object),
            _featureToggleHelper.Object,
            new EmptyValidator(),
            _userContextProvider.Object,
            _mockLogger.Object,
            _metricsRecorder.Object
        );
    }

    [Fact(DisplayName = "Returns NotImplemented when toggled OFF")]
    public async Task RunAsync_ReturnsNotImplemented_WhenToggledOff()
    {
        //Arrange
        _featureToggleHelper
            .Setup(helper => helper.IsFeatureEnabled(Flags.ReportsUplift))
            .ReturnsAsync(false);

        //Act
        var result = await _sut.RunAsync(CreateRequest());

        //Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        contentResult.StatusCode.Should().Be(501);
        contentResult.Content.Should().Contain("Master Site List Reports are not enabled.");
    }

    [Fact(DisplayName = "Generates master site list reports")]
    public async Task RunAsync_Generates_Report()
    {
        //Arrange
        var sites = new List<Site>();
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user2@testdomain.com");
        
        _featureToggleHelper
            .Setup(helper => helper.IsFeatureEnabled(Flags.ReportsUplift))
            .ReturnsAsync(true);
        
        _userContextProvider.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);

        _siteService.Setup(x => x.GetAllSites(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(sites);

        //Act
        var result = await _sut.RunAsync(CreateRequest());
        var fileContentResult = Assert.IsType<FileContentResult>(result);

        fileContentResult.ContentType.Should().Be("text/csv");
        fileContentResult.FileDownloadName.Should().Be("MasterSiteListReport_00010101120000.csv");

        // Assert on csv headers
        var contentString = Encoding.UTF8.GetString(fileContentResult.FileContents);
        var csvLines = contentString.Split(Environment.NewLine);
        var headers = csvLines[0].Split(',');

        headers.Length.Should().Be(11);
        headers.Should().Contain("Site Name");
        headers.Should().Contain("ODS Code");
        headers.Should().Contain("Site Type");
        headers.Should().Contain("Region");
        headers.Should().Contain("ICB");
        headers.Should().Contain("GUID");
        headers.Should().Contain("IsDeleted");
        headers.Should().Contain("Status");
        headers.Should().Contain("Long");
        headers.Should().Contain("Lat");
        headers.Should().Contain("Address");
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
