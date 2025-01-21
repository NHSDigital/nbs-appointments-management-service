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
using System.Security.Principal;

namespace Nhs.Appointments.Api.Tests.Functions
{
    public class GetUserProfileFunctionTests
    {
        private readonly Mock<IUserService> _userSiteAssignmentService = new();
        private readonly Mock<IValidator<EmptyRequest>> _validator = new();
        private readonly Mock<IUserContextProvider> _userContextProvider = new();
        private readonly Mock<ILogger<GetUserProfileFunction>> _logger = new();
        private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
        private GetUserProfileFunction _sut;

        public GetUserProfileFunctionTests()
        {
            _sut = new GetUserProfileFunction(_userSiteAssignmentService.Object, _validator.Object, _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);
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
            var actualResponse = await ReadResponseAsync<UserProfile>(response.Content);
            actualResponse.EmailAddress.Should().Be("test@test.com");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RunAsync_ReturnsCorrectHasSites(bool hasSites)
        {
            var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
            _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
            var context = new DefaultHttpContext();
            var request = context.Request;

            RoleAssignment[] roleAssignments = [];

            if (hasSites)
            {
                roleAssignments = [
                    new() { Role = "Role1", Scope = "site:1" },
                    new() { Role = "system:admin-user", Scope = "global" }
                ];
            } 

            _userSiteAssignmentService.Setup(x => x.GetUserAsync("test@test.com")).ReturnsAsync(new User()
            {
                Id = "test@test.com",
                RoleAssignments = roleAssignments,
            });

            var siteDetails = new[]
{
                new Site("1", "Alpha", "somewhere", "0113 1111111", "R1", "ICB1",new [] {new AttributeValue(Id: "Attribute 1", Value: "true")}, new Location("point", new []{0.1, 10})),
                new Site("2", "Beta", "somewhere else", "0113 222222", "R2", "ICB2",new [] {new AttributeValue(Id: "Attribute 2", Value: "true")}, new Location("point", new []{0.2, 11}))
            };

            var result = await _sut.RunAsync(request) as ContentResult;

            var actualResponse = await ReadResponseAsync<UserProfile>(result.Content);
            actualResponse.hasSites.Should().Be(hasSites);
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
