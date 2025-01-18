using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetUserRoleAssignmentsFunctionTests
{
    private readonly Mock<ILogger<GetUserRoleAssignmentsFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly GetUserRoleAssignmentsFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IValidator<SiteBasedResourceRequest>> _validator = new();

    public GetUserRoleAssignmentsFunctionTests()
    {
        _sut = new GetUserRoleAssignmentsFunction(_userService.Object, _validator.Object, _userContextProvider.Object,
            _logger.Object, _metricsRecorder.Object);
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<SiteBasedResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsUserRoleAssignments_ForTheRequestedSite()
    {
        var users = new User[]
        {
            new()
            {
                Id = "test1@test.com",
                RoleAssignments = [new RoleAssignment { Role = "Role1", Scope = "site:2de5bb57-060f-4cb5-b14d-16587d0c2e8f" }]
            },
            new()
            {
                Id = "test2@test.com",
                RoleAssignments =
                [
                    new RoleAssignment { Role = "Role1", Scope = "site:2de5bb57-060f-4cb5-b14d-16587d0c2e8f" },
                    new RoleAssignment { Role = "Role1", Scope = "site:308d515c-2002-450e-b248-4ba36f6667bb" }
                ]
            }
        };
        var expectedResult = new User[]
        {
            new()
            {
                Id = "test1@test.com",
                RoleAssignments = [new RoleAssignment { Role = "Role1", Scope = "site:2de5bb57-060f-4cb5-b14d-16587d0c2e8f" }]
            },
            new()
            {
                Id = "test2@test.com",
                RoleAssignments = [new RoleAssignment { Role = "Role1", Scope = "site:2de5bb57-060f-4cb5-b14d-16587d0c2e8f" }]
            }
        };
        _userService.Setup(x => x.GetUsersAsync("2de5bb57-060f-4cb5-b14d-16587d0c2e8f")).ReturnsAsync(users);
        var request = CreateRequest();
        var result = await _sut.RunAsync(request) as ContentResult;
        result?.StatusCode.Should().Be(200);
        var actualResponse = await ReadResponseAsync<IEnumerable<User>>(result.Content);
        actualResponse.Should().BeEquivalentTo(expectedResult);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.QueryString = new QueryString("?site=2de5bb57-060f-4cb5-b14d-16587d0c2e8f");
        return request;
    }


    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}
