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

namespace Nhs.Appointments.Api.Tests.Functions;
public class GetSitesPreviewFunctionTests
{
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IUserService> _userSiteAssignmentService = new();
    private readonly Mock<IValidator<EmptyRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetSitesPreviewFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private GetSitesPreviewFunction _sut;

    public GetSitesPreviewFunctionTests()
    {
        _sut = new GetSitesPreviewFunction(_siteService.Object, _userSiteAssignmentService.Object, _validator.Object, _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);
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
    public async Task RunsAsync_UserNotFound_ReturnsFailedResponse()
    {
        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        var context = new DefaultHttpContext();
        var request = context.Request;
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(() => null);

        var response = await _sut.RunAsync(request) as ContentResult;
        

        response.StatusCode.Should().Be(404);
        response.Content.Should().Contain("User was not found");
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
            new("1", "Site1"),
            new("2", "Site2")
        };
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(new User()
        {
            Id = "test@test.com",
            RoleAssignments = roleAssignments
        });
        _siteService.Setup(x => x.GetSitesPreview()).ReturnsAsync(sitesPreview);

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
        var site = new Site(
            Id: "1",
            Name: "Alpha",
            Address: "somewhere",
            PhoneNumber: "0113 1111111",
            OdsCode: "odsCode1",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            AttributeValues: new[] { new AttributeValue(Id: "accessibility/attr_1", Value: "true") },
            Location: new Location("point", [0.1, 10])
        );


        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(new User()
        {
            Id = "test@test.com",
            RoleAssignments = roleAssignments
        });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(site);

        var response = await _sut.RunAsync(request) as ContentResult;
        var actualResponse = await ReadResponseAsync<IEnumerable<SitePreview>>(response.Content);

        actualResponse.Count().Should().Be(1);
        actualResponse.First().Id.Should().Be("1");
        actualResponse.First().Name.Should().Be("Alpha");
        _siteService.Verify(x => x.GetSitesPreview(), Times.Never);
        _siteService.Verify(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}
