using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetSitesPreviewFunctionTests
{
    private readonly Mock<ILogger<GetSitesPreviewFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IPermissionChecker> _permissionChecker = new();
    private readonly GetSitesPreviewFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IUserService> _userSiteAssignmentService = new();
    private readonly Mock<IValidator<EmptyRequest>> _validator = new();
    private readonly Mock<IWellKnowOdsCodesService> _wellKnowOdsCodesService = new();

    public GetSitesPreviewFunctionTests()
    {
        _sut = new GetSitesPreviewFunction(
            _userSiteAssignmentService.Object, 
            _validator.Object,
            _userContextProvider.Object, 
            _logger.Object, 
            _metricsRecorder.Object, 
            _permissionChecker.Object, 
            _wellKnowOdsCodesService.Object
        );
    }

    [Fact]
    public async Task RunsAsync_GetsRoleAssignments_ForSignedInUser()
    {
        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        var context = new DefaultHttpContext();
        var request = context.Request;

        await _sut.RunAsync(request);

        _userSiteAssignmentService.Verify(x => x.GetUserAsync("test@test.com"), Times.Once());
    }

    [Fact]
    public async Task RunsAsync_GetsCorrectEmailAdress_ForSignedInUser()
    {
        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        var context = new DefaultHttpContext();
        var request = context.Request;

        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(new User
        {
            Id = "test@test.com", RoleAssignments = [],
        });

        var response = await _sut.RunAsync(request) as ContentResult;

        _userSiteAssignmentService.Verify(x => x.GetUserAsync(It.Is<string>(email => email == "test@test.com")),
            Times.Once);
    }

    [Fact]
    public async Task RunsAsync_UserNotFound_ReturnsEmptyResponse()
    {
        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        var context = new DefaultHttpContext();
        var request = context.Request;
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(() => null);

        var response = await _sut.RunAsync(request) as ContentResult;
        var actualResponse = await ReadResponseAsync<IEnumerable<SitePreview>>(response.Content);

        response.StatusCode.Should().Be(200);
        actualResponse.Count().Should().Be(0);
    }

    [Fact]
    public async Task RunsAsync_UserFound_GetsUsersSites()
    {
        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        var context = new DefaultHttpContext();
        var request = context.Request;
        var roleAssignments = new RoleAssignment[] { new() { Role = "Role1", Scope = "site:1" } };
        var site1 = new Site(
            Id: "1",
            Name: "Alpha",
            Address: "somewhere",
            PhoneNumber: "0113 1111111",
            OdsCode: "odsCode1",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            InformationForCitizens: "Information For Citizens 123456",
            Accessibilities: new[] { new Accessibility(Id: "accessibility/attr_1", Value: "true") },
            Location: new Location("point", [0.1, 10]),
            status: SiteStatus.Online
        );

        var site2 = new Site(
            Id: "3",
            Name: "Gamma",
            Address: "somewhere",
            PhoneNumber: "0113 333333",
            OdsCode: "odsCode3",
            Region: "R1",
            IntegratedCareBoard: "ICB3",
            InformationForCitizens: "Information For Citizens 123456",
            Accessibilities: new[] { new Accessibility(Id: "accessibility/attr_1", Value: "true") },
            Location: new Location("point", [0.1, 10]),
            status: SiteStatus.Online
        );

        var icbs = new List<WellKnownOdsEntry>
        {
            new("ICB1", "ICB One", "icb"),
            new("ICB3", "ICB Three", "icb")
        };
        
        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(new User
        {
            Id = "test@test.com", RoleAssignments = roleAssignments
        });
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        _permissionChecker.Setup(x => x.GetSitesWithPermissionAsync("test@test.com", Permissions.ViewSitePreview))
            .ReturnsAsync(new List<Site> { site1, site2});
        _wellKnowOdsCodesService.Setup(x => x.GetWellKnownOdsCodeEntries()).ReturnsAsync(icbs);

        var response = await _sut.RunAsync(request) as ContentResult;
        var actualResponse = await ReadResponseAsync<IEnumerable<SitePreview>>(response.Content);

        actualResponse.Count().Should().Be(2);
        actualResponse.First().Id.Should().Be("1");
        actualResponse.First().Name.Should().Be("Alpha");
        actualResponse.First().IntegratedCareBoard.Should().Be("ICB One");

        actualResponse.Last().Id.Should().Be("3");
        actualResponse.Last().Name.Should().Be("Gamma");
        actualResponse.Last().IntegratedCareBoard.Should().Be("ICB Three");
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}
