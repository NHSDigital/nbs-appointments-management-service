using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Functions;
public class GetSitesPreviewFunctionTests
{
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IUserService> _userSiteAssignmentService = new();
    private readonly Mock<IValidator<EmptyRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetSitesPreviewFunction>> _logger = new();
    private readonly Mock<IPermissionChecker> _permissionChecker = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private GetSitesPreviewFunction _sut;

    public GetSitesPreviewFunctionTests()
    {
        _sut = new GetSitesPreviewFunction(_siteService.Object, _userSiteAssignmentService.Object, _validator.Object, _userContextProvider.Object, _logger.Object, _metricsRecorder.Object, _permissionChecker.Object);
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

        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(new User()
        {
            Id = "test@test.com",
            RoleAssignments = [],
        });

        var response = await _sut.RunAsync(request) as ContentResult;

        _userSiteAssignmentService.Verify(x => x.GetUserAsync(It.Is<string>(email => email == "test@test.com")), Times.Once);
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
        _siteService.Verify(x => x.GetSitesPreview(), Times.Never);
        _siteService.Verify(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RunsAsync_IsAdminUser_GetsAllSitesPreview()
    {
        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        var context = new DefaultHttpContext();
        var request = context.Request;
        var roleAssignments = new RoleAssignment[] {
            new(){ Role = "Role1", Scope = "site:1" },
            new(){ Role = "system:admin-user", Scope = "global" }
        };
        var sitesPreview = new SitePreview[] {
            new("1", "Site1", "ODS1"),
            new("2", "Site2", "ODS2"),
        };
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(new User()
        {
            Id = "test@test.com",
            RoleAssignments = roleAssignments
        });
        _siteService.Setup(x => x.GetSitesPreview()).ReturnsAsync(sitesPreview);
        _permissionChecker.Setup(x => x.HasGlobalPermissionAsync("test@test.com", Permissions.ViewSitePreview)).ReturnsAsync(true);

        var response = await _sut.RunAsync(request) as ContentResult;
        var actualResponse = await ReadResponseAsync<IEnumerable<SitePreview>>(response.Content);

        actualResponse.Count().Should().Be(2);
        _siteService.Verify(x => x.GetSitesPreview(), Times.Once);
        _siteService.Verify(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RunsAsync_IsNotAdmin_GetsUsersSites()
    {
        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        var context = new DefaultHttpContext();
        var request = context.Request;
        var roleAssignments = new RoleAssignment[] {
            new(){ Role = "Role1", Scope = "site:1" }
        };
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
            Location: new Location("point", [0.1, 10])
        );
        
        var site2 = new Site(
            Id: "2",
            Name: "Beta",
            Address: "somewhere",
            PhoneNumber: "0113 22222222",
            OdsCode: "odsCode2",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            InformationForCitizens: "Information For Citizens 123456",
            Accessibilities: new[] { new Accessibility(Id: "accessibility/attr_1", Value: "true") },
            Location: new Location("point", [0.1, 10])
        );
        
        var site3 = new Site(
            Id: "3",
            Name: "Gamma",
            Address: "somewhere",
            PhoneNumber: "0113 333333",
            OdsCode: "odsCode3",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            InformationForCitizens: "Information For Citizens 123456",
            Accessibilities: new[] { new Accessibility(Id: "accessibility/attr_1", Value: "true") },
            Location: new Location("point", [0.1, 10])
        );
        
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(new User()
        {
            Id = "test@test.com",
            RoleAssignments = roleAssignments
        });
        
        _siteService.Setup(x => x.GetSiteByIdAsync(site1.Id, It.IsAny<string>())).ReturnsAsync(site1);
        _siteService.Setup(x => x.GetSiteByIdAsync(site2.Id, It.IsAny<string>())).ReturnsAsync(site2);
        _siteService.Setup(x => x.GetSiteByIdAsync(site3.Id, It.IsAny<string>())).ReturnsAsync(site3);
        
        _permissionChecker.Setup(x=> x.GetSitesWithPermissionAsync("test@test.com", Permissions.ViewSitePreview))
            .ReturnsAsync(new List<string>() {site1.Id, site3.Id});

        var response = await _sut.RunAsync(request) as ContentResult;
        var actualResponse = await ReadResponseAsync<IEnumerable<SitePreview>>(response.Content);

        actualResponse.Count().Should().Be(2);
        actualResponse.First().Id.Should().Be("1");
        actualResponse.First().Name.Should().Be("Alpha");
        actualResponse.Last().Id.Should().Be("3");
        actualResponse.Last().Name.Should().Be("Gamma");
        _siteService.Verify(x => x.GetSitesPreview(), Times.Never);
        _siteService.Verify(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}
