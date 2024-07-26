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

namespace Nhs.Appointments.Api.Tests.Functions
{
    public class GetUsersForSiteFunctionTests
    {
        private readonly Mock<IUserService> _userService = new();
        private readonly Mock<IValidator<SiteBasedResourceRequest>> _validator = new();
        private readonly Mock<IUserContextProvider> _userContextProvider = new();
        private readonly Mock<ILogger<GetUsersForSiteFunction>> _logger = new();

        private static HttpRequest CreateRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.QueryString = new QueryString($"?site=1000");
            return request;
        }

        [Fact]
        public async Task RunsAsync_GetsUsersForSite()
        {
            var roleAssignments = new List<RoleAssignment>()
            {
                new() { Role = "users:view", Scope = "site:1000" },
            };
            _userService.Setup(x => x.GetUserRoleAssignments("test@test.com")).ReturnsAsync(roleAssignments);

            var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
            _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
            var request = CreateRequest();

            var sut = new GetUsersForSiteFunction(_userService.Object, _validator.Object, _userContextProvider.Object, _logger.Object);
            await sut.RunAsync(request);

            _userService.Verify(x => x.GetUsersForSite("1000"), Times.Once());
        }

        [Fact]
        public async Task RunsAsync_DoesNotGetUsersIfMissingPermissions()
        {
            var roleAssignments = new List<RoleAssignment>()
            {
                new() { Role = "not-the-required-role", Scope = "site:not-the-correct-site" },
                new() { Role = "users:view", Scope = "site:not-the-correct-site" },
                new() { Role = "not-the-required-role", Scope = "site:1000" },
            };
            _userService.Setup(x => x.GetUserRoleAssignments("test@test.com")).ReturnsAsync(roleAssignments);

            var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
            _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
            var request = CreateRequest();

            var sut = new GetUsersForSiteFunction(_userService.Object, _validator.Object, _userContextProvider.Object, _logger.Object);
            var response = await sut.RunAsync(request) as ContentResult;

            _userService.Verify(x => x.GetUsersForSite("1000"), Times.Never());
            response?.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task RunsAsync_OmitsUserRolesForOtherSites()
        {
            var roleAssignments = new List<RoleAssignment>()
            {
                new() { Role = "users:view", Scope = "site:1000" },
            };
            _userService.Setup(x => x.GetUserRoleAssignments("test@test.com")).ReturnsAsync(roleAssignments);
            _userService.Setup(x => x.GetUsersForSite("1000")).ReturnsAsync(new User[]
            {
                new()
                {
                    Id = "user.one@nhs.net",
                    RoleAssignments =
                    [
                        new()
                        {
                            Role = "canned:api-user",
                            Scope = "site:1000"
                        },
                        new()
                        {
                            Role = "canned:site-configuration-manager",
                            Scope = "site:1000"
                        },
                        new()
                        {
                            Role = "canned:api-user",
                            Scope = "site:1001"
                        }
                    ]
                }
            });

            var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
            _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
            var request = CreateRequest();

            var sut = new GetUsersForSiteFunction(_userService.Object, _validator.Object, _userContextProvider.Object,
                _logger.Object);
            var result = await sut.RunAsync(request) as ContentResult;

            _userService.Verify(x => x.GetUsersForSite("1000"), Times.Once());

            var response = ReadResponseAsync<GetUsersForSiteResponse>(result.Content);
            response.Result.Users.Should().HaveCount(1);
            response.Result.Users.Single().RoleAssignments.Should().BeEquivalentTo(new RoleAssignment[]
            {
                new()
                {
                    Role = "canned:api-user",
                    Scope = "site:1000"
                },
                new()
                {
                    Role = "canned:site-configuration-manager",
                    Scope = "site:1000"
                }
            });
        }
        
        private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
        {
            var body = await new StringReader(response).ReadToEndAsync();
            return JsonConvert.DeserializeObject<TRequest>(body);
        }
    }
}
