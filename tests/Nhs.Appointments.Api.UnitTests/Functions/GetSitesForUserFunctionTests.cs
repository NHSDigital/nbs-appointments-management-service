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
using System.Security.Claims;
using System.Security.Principal;

namespace Nhs.Appointments.Api.Tests.Functions
{
    public class GetSitesForUserFunctionTests
    {
        private readonly Mock<ISiteSearchService> _siteSearchService = new();
        private readonly Mock<IUserService> _userSiteAssignmentService = new();
        private readonly Mock<IValidator<EmptyRequest>> _validator = new();
        private readonly Mock<IUserContextProvider> _userContextProvider = new();
        private readonly Mock<ILogger<GetSitesForUserFunction>> _logger = new();

        [Fact]
        public async Task RunsAsync_GetsAssignments_ForSignedInUser()
        {            
            var testPrincipal = CreateUserPrincipal("test@test.com");
            _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
            var context = new DefaultHttpContext();
            var request = context.Request;

            var sut = new GetSitesForUserFunction(_siteSearchService.Object, _userSiteAssignmentService.Object, _validator.Object, _userContextProvider.Object, _logger.Object);
            await sut.RunAsync(request);

            _userSiteAssignmentService.Verify(x => x.GetUserRoleAssignments("test@test.com"), Times.Once());
        }

        [Fact]
        public async Task RunsAsync_GetsCorrectSites_ForSignedInUser()
        {
            var testPrincipal = CreateUserPrincipal("test@test.com");
            _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
            var context = new DefaultHttpContext();
            var request = context.Request;
            var roleAssignments = new List<RoleAssignment>()
            {
                new() { Role = "Role1", Scope = "site:1" },
                new() { Role = "Role2", Scope = "site:1" },
                new() { Role = "Role1", Scope = "site:2" },
                new() { Role = "Role2", Scope = "site:2" }
            };
            _userSiteAssignmentService.Setup(x => x.GetUserRoleAssignments("test@test.com")).ReturnsAsync(roleAssignments);

            var siteDetails = new[]
            {
                new Site("1", "Alpha", "somewhere"),
                new Site("2", "Beta", "elsewhere"),
            };

            _siteSearchService.Setup(x => x.GetSiteByIdAsync("1")).ReturnsAsync(siteDetails[0]);
            _siteSearchService.Setup(x => x.GetSiteByIdAsync("2")).ReturnsAsync(siteDetails[1]);

            var sut = new GetSitesForUserFunction(_siteSearchService.Object, _userSiteAssignmentService.Object, _validator.Object, _userContextProvider.Object, _logger.Object);
            var result = await sut.RunAsync(request) as ContentResult;

            var actualResponse = await ReadResponseAsync<Site[]>(result.Content);
            actualResponse.Should().BeEquivalentTo(siteDetails);
        }
        
        [Fact]
        public async Task RunAsync_IgnoresScopesNotPrefixedWithSite_ForSignedInUser()
        {
            var testPrincipal = CreateUserPrincipal("test@test.com");
            _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
            var context = new DefaultHttpContext();
            var request = context.Request;
            var roleAssignments = new List<RoleAssignment>()
            {
                new() { Role = "Role1", Scope = "global" },
                new() { Role = "Role1", Scope = "site:1" }
            };
            _userSiteAssignmentService.Setup(x => x.GetUserRoleAssignments("test@test.com")).ReturnsAsync(roleAssignments);

            var siteDetails = new[]
            {
                new Site("1", "Alpha", "somewhere"),
            };

            _siteSearchService.Setup(x => x.GetSiteByIdAsync("1")).ReturnsAsync(siteDetails[0]);

            var sut = new GetSitesForUserFunction(_siteSearchService.Object, _userSiteAssignmentService.Object, _validator.Object, _userContextProvider.Object, _logger.Object);
            var result = await sut.RunAsync(request) as ContentResult;

            var actualResponse = await ReadResponseAsync<Site[]>(result.Content);
            actualResponse.Should().BeEquivalentTo(siteDetails);
        }

        private ClaimsPrincipal CreateUserPrincipal(string emailAddress)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, emailAddress),                
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            return new ClaimsPrincipal(identity);
        }

        private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
        {
            var body = await new StringReader(response).ReadToEndAsync();
            return JsonConvert.DeserializeObject<TRequest>(body);
        }
    }

    public class TestIdentity : IIdentity
    {
        public string AuthenticationType => "Test";

        public bool IsAuthenticated => true;

        public string Name => "Test User";
    }
}
